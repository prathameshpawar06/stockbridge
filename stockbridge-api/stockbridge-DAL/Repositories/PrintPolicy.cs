using System.Data;
using System.Globalization;
using Dapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.Repositories
{
    public class PrintPolicy
    {
        private readonly StockbridgeContext _context;

        public PrintPolicy(StockbridgeContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateWordDocumentAsync(int policy_Id)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                    {
                        MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document(new Body());
                        Body body = mainPart.Document.Body;
                        body.Append(CreateNarrowPageLayout());

                        var policydata = await _context.Policies.FirstOrDefaultAsync(x => x.PolicyId == policy_Id);

                        var client = await _context.Clients.FirstOrDefaultAsync(x => x.ClientId == policydata.ClientId);
                        var clientName = client?.CompanyName ?? "";

                        // Add styles
                        AddStylesPartToDocument(mainPart);

                        var policies = await _context.Policies
                            .Where(x => x.PolicyId == policy_Id)
                            .OrderBy(x => x.PrintSequence)
                            .Include(x => x.Client)
                            .Include(x => x.Carrier)
                            .Include(x => x.Broker)
                            .Select(x => new
                            {
                                Policy = x,
                                ClientName = x.Client.CompanyName,
                                CarrierName = x.Carrier.Name,
                                BrokerName = x.Broker.Name
                            })
                            .FirstOrDefaultAsync();

                        var clientEntity = await _context.PolicyEntities
                            .Where(x => x.PolicyId == policy_Id && x.NamedInsured)
                            .OrderBy(x => x.Entity.Name)
                            .Include(x => x.Entity)
                            .Select(x => new
                            {
                                PolicyEntity = x,
                                EntityName = x.Entity.Name,
                                Sequence = x.Entity.Sequence
                            })
                            .ToListAsync();

                        var policyLocations = await _context.PolicyLocations
                            .Where(x => x.PolicyId == policy_Id)
                            .OrderBy(x => x.Location.State)
                            .Include(x => x.Location)
                            .Select(x => new
                            {
                                PolicyEntity = x,
                                LocationSequence = x.Location.Sequence,
                                LocationAddress = x.Location.Address1,
                                LocationCity = x.Location.City,
                                LocationState = x.Location.State,
                            })
                            .ToListAsync();
                        var policymajors = _context.PolicyMajors.OrderBy(x => x.Sequence);
                        var majorColDefs = _context.PolicyMajorColDefs.OrderBy(x => x.Sequence);
                        IQueryable<PolicyMinorDef> minorColDefs = _context.PolicyMinorDefs.OrderBy(x => x.RowSequence);

                        var policyMajorClientData = await GetPolicyMajorDataAsync(policydata.ClientId);

                        string[] sections = await GetTableOfContent(policy_Id);

                        var templateprincipale = await _context.TemplatePrincipals.FirstOrDefaultAsync(x => x.PrincipalId == policydata.PrincipalId);

                        string title = (templateprincipale.Description ?? "")
                                        + (string.IsNullOrWhiteSpace(policydata.PolicyTitle) ? "" : " --- " + policydata.PolicyTitle);


                        AddSection(body, title, policies, policy_Id,
                                   clientEntity.ToArray(), policyLocations.ToArray(), policy_Id, policymajors, majorColDefs, minorColDefs, policyMajorClientData);
                        // Ensure TOC updates when opening the document
                        AddUpdateFieldsOnOpen(mainPart);
                        AddPagination(mainPart, clientName, templateprincipale.Description ?? "", policydata.PolicyNo);
                        mainPart.Document.Save();
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static SectionProperties CreateNarrowPageLayout()
        {
            return new SectionProperties(
                new PageSize() { Width = 12240, Height = 15840 }, // A4 size
                new PageMargin()
                {
                    Top = 720,    // 0.25 inch
                    Bottom = 720, // 0.25 inch
                    Left = 720,   // 0.25 inch
                    Right = 720,  // 0.25 inch
                    Header = 450,
                    Footer = 450,
                    Gutter = 0
                }
            );
        }

        // Helper Method for Styling
        private void ApplyParagraphStyling(Paragraph paragraph, JustificationValues justification, string fontSize, bool bold, string colorHex)
        {
            ParagraphProperties pp = new ParagraphProperties();
            pp.Justification = new Justification() { Val = justification };
            paragraph.PrependChild(pp);

            RunProperties rp = new RunProperties();
            rp.FontSize = new FontSize() { Val = fontSize };
            if (bold) rp.Bold = new Bold();
            rp.Color = new Color() { Val = colorHex }; // Set text color

            paragraph.GetFirstChild<Run>().PrependChild(rp);
        }

        // Helper Method to Add Empty Lines
        private void AddEmptyLines(Body body, int count)
        {
            for (int i = 0; i < count; i++)
            {
                body.Append(new Paragraph(new Run(new Text(" "))));
            }
        }


        private async Task<string[]> GetTableOfContent(int policy_Id)
        {
            string[] sections = {
                        "Legal Entity Listing",
                        "Location Listing",
                        "Expiration/Premium Summary"
                    };

            var policies = await _context.Policies
                .Where(x => x.PolicyId == policy_Id && !x.Expired)
                .OrderBy(x => x.PrintSequence)
                .Select(x => $"{x.PolicyId}:{x.PolicyTitle}:{x.Principal.Description}")
                .ToListAsync();

            return sections.Concat(policies).ToArray();
        }

        static void AddFooter(Body body)
        {
            // Horizontal line
            Paragraph horizontalLine = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Left }),
                new Run(new Text("_________________________________________________________________________________"))
            );

            // Footer text
            Paragraph footerText = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Left }),
                new Run(
                    new RunProperties(new FontSize() { Val = "18" }),
                    new Text("Ditmas Management Corporation")
                )
            );

            // Append footer elements
            body.AppendChild(horizontalLine);
            body.AppendChild(footerText);
        }

        static void AddStylesPartToDocument(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.StyleDefinitionsPart ?? mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles(
                new Style(
                    new StyleName() { Val = "Heading 1" },
                    new BasedOn() { Val = "Normal" },
                    new NextParagraphStyle() { Val = "Normal" },
                    new UIPriority() { Val = 9 },
                    new PrimaryStyle(),
                    new StyleRunProperties(
                        new FontSize() { Val = "18" } // 14pt
                    )
                )
                {
                    Type = StyleValues.Paragraph,
                    StyleId = "Heading1"
                }
            );
            stylesPart.Styles.Save();
        }

        static void AddUpdateFieldsOnOpen(MainDocumentPart mainPart)
        {
            if (mainPart.DocumentSettingsPart == null)
                mainPart.AddNewPart<DocumentSettingsPart>();

            mainPart.DocumentSettingsPart.Settings = new Settings(
                new UpdateFieldsOnOpen() { Val = true }
            );
            mainPart.DocumentSettingsPart.Settings.Save();
        }

        private async void AddSection(Body body, string sectionTitle, dynamic policiesData,
            int? policyId, dynamic[] clientEntities, dynamic[] policyLocations, int clientId, IQueryable<PolicyMajor> policyMajorData,
            IQueryable<PolicyMajorColDef> policyMajorColDefData, IQueryable<PolicyMinorDef> policyminorDefs, List<PolicyMajorData> policyMajorClientData)
        {

            // Create the heading
            Paragraph heading = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Left },
                    new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto },
                    new ParagraphStyleId() { Val = "Heading1" }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Aptos Display" },
                        new FontSize() { Val = "38" }, // Size 40px (80 half-points)
                        new Color() { Val = "1F4E78" }, //1F4E78 
                        new Underline() { Val = UnderlineValues.Single } // Add underline
                    ),
                    new Text(sectionTitle)
                )
            );

            // Append the heading and content to the document
            body.AppendChild(heading);

            // Add one empty line (empty paragraph)
            Paragraph emptyLine = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto }
                ),
                new Run(new Text("")) // Empty text creates a blank line
            );

            // Append the empty line
            body.AppendChild(emptyLine);

            // Policy Summary Detail Section
            var policy = policiesData;

            if (policy != null)
            {
                Table table = new Table();

                // Define table properties
                TableProperties tblProperties = new TableProperties(
                    new TableWidth() { Width = "11000", Type = TableWidthUnitValues.Dxa },
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Double, Size = 8 },
                        new BottomBorder { Val = BorderValues.Double, Size = 8 },
                        new LeftBorder { Val = BorderValues.Double, Size = 8 },
                        new RightBorder { Val = BorderValues.Double, Size = 8 }
                    ),
                    new TableCellMarginDefault(  // Add margin (spacing inside cells)
                        new TopMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                        //new BottomMargin { Width = "200", Type = TableWidthUnitValues.Dxa },
                        new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },   // Adjust left margin
                        new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa }   // Adjust right margin
                    )
                );
                table.AppendChild(tblProperties);

                TableRow CreateRow(string label1, string value1, string label2, string value2)
                {
                    TableRow tr = new TableRow();
                    tr.Append(
                        new TableCell(
                            new TableCellProperties(new TableWidth { Type = TableWidthUnitValues.Dxa, Width = "2000" }),  // Wider first column
                            new Paragraph(new Run(new Text(string.IsNullOrWhiteSpace(label1) ? "" : label1 + ":")))
                        ),
                        new TableCell(
                            new TableCellProperties(new TableWidth { Type = TableWidthUnitValues.Dxa, Width = "3000" }),  // Wider second column
                            new Paragraph(new Run(new Text(value1)))
                        ),
                        new TableCell(
                            new TableCellProperties(new TableWidth { Type = TableWidthUnitValues.Dxa, Width = "2000" }),  // Wider third column
                            new Paragraph(new Run(new Text(string.IsNullOrWhiteSpace(label2) ? "" : label2 + ": ")))
                        ),
                        new TableCell(
                            new TableCellProperties(new TableWidth { Type = TableWidthUnitValues.Dxa, Width = "3000" }),  // Wider second column
                            new Paragraph(new Run(new Text(value2)))
                        )
                    );
                    return tr;
                }


                // Create rows with a structured layout (moving Expiration, Premium, and Auditable to a new column)
                table.Append(
                    CreateRow("Client", policy.ClientName, "", ""),
                    CreateRow("Broker", policy.BrokerName, "", ""),
                    CreateRow("Carrier", policy.CarrierName, "Last Updated", policy.Policy.ChangeDate?.ToString("MM/dd/yyyy") ?? ""),
                    CreateRow("Policy Number", policy.Policy.PolicyNo, "A.M.Best Rating", "NR"),
                    CreateRow("Inception", policy.Policy.InceptionDate?.ToString("MM/dd/yyyy") ?? "", "Expiration", policy.Policy.ExpirationDate?.ToString("MM/dd/yyyy") ?? ""),
                    CreateRow("Annual Premium", policy.Policy.AnualPremium?.ToString("C0", CultureInfo.GetCultureInfo("en-US")) ?? "", "Auditable", policy.Policy.Audit ? "Yes" : "No")
                );

                // Append the table to the document body
                body.AppendChild(table);

                // add policy comment
                // Check if PolicyComment is null or empty before processing
                if (!string.IsNullOrWhiteSpace(policy.Policy.PolicyComment))
                {

                    // add empty row for spacing
                    body.AppendChild(new Paragraph(new Run(new Text(""))));

                    // Split the PolicyComment into individual items using newline as a separator
                    var comments = policy.Policy.PolicyComment.Split('\n');

                    foreach (var comment in comments)
                    {
                        if (string.IsNullOrWhiteSpace(comment))
                        {
                            continue;
                        }

                        // Create a paragraph with a bullet for each comment
                        var paragraphdata = new Paragraph(
                            new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Left } // Optional alignment
                            ),
                            new Run(
                                new Text("- " + comment.Trim()) // Add "-" as a bullet and trim spaces
                            )
                        );

                        // Add the bullet item to the document body
                        body.AppendChild(paragraphdata);
                    }
                }
                //else
                //{
                //    // Optional: Handle the case where PolicyComment is null or empty
                //    var emptyMessage = new Paragraph(
                //        new Run(
                //            new Text(" ") // Fallback text
                //        )
                //    );
                //    body.AppendChild(emptyMessage);
                //}

                // end policy Comment

                // Add Name insured
                if (!policy.Policy.SuppressNamedInsureds)
                {
                    var entities = clientEntities.Where(x => x.PolicyEntity.PolicyId == policyId).ToList();
                    if (entities.Any())
                    {
                        body.AppendChild(new Paragraph(new Run(new Text(""))));
                        Paragraph paragraph = new Paragraph(
                            new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Left } // Left-align text
                            ),
                            new Run(
                                new RunProperties(
                                    new Bold(),  // Make text bold
                                    new Underline() { Val = UnderlineValues.Single },
                                    new RunFonts() { Ascii = "Arial" },
                                    new FontSize() { Val = "22" }
                                ),
                                new Text("Name Insured: ")
                            ));
                        body.AppendChild(paragraph);

                        Table nameInsuredtable = new Table();

                        // Define table properties
                        TableProperties nameInsuredtableProperties = new TableProperties(
                            new TableBorders(
                                new TopBorder() { Val = BorderValues.Nil },
                                new BottomBorder() { Val = BorderValues.Nil },
                                new LeftBorder() { Val = BorderValues.Nil },
                                new RightBorder() { Val = BorderValues.Nil },
                                new InsideHorizontalBorder() { Val = BorderValues.Nil },
                                new InsideVerticalBorder() { Val = BorderValues.Nil }
                            )
                        );
                        nameInsuredtable.AppendChild(nameInsuredtableProperties);

                        // Create the header row
                        TableRow headerRow = new TableRow();
                        headerRow.Append(
                            CreateTableCell("Ent #", false, "800"),
                            CreateTableCell("Name", false, "5000")
                        );
                        nameInsuredtable.Append(headerRow);

                        // Create rows for each location
                        foreach (var e in entities)
                        {
                            TableRow row = new TableRow();
                            row.Append(
                                CreateTableCell(e.Sequence.ToString(), false, "800"),
                                CreateTableCell(e.EntityName, false, "5000")
                            );
                            nameInsuredtable.Append(row);
                        }

                        body.AppendChild(nameInsuredtable);
                    }
                }

                // end Name Insured

                // Add Location Section
                if (!policy.Policy.SuppressLocations)
                {
                    
                    var policyLocation = policyLocations
                                        .Where(x => x.PolicyEntity.PolicyId == policyId)
                                        .ToList();
                    if (policyLocation.Any())
                    {
                        body.AppendChild(new Paragraph(new Run(new Text(""))));
                        Paragraph locationParagraph = new Paragraph(
                            new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Left } // Left-align text
                            ),
                            new Run(
                                new RunProperties(
                                    new Bold(),  // Make text bold
                                    new Underline() { Val = UnderlineValues.Single },
                                    new RunFonts() { Ascii = "Arial" },
                                    new FontSize() { Val = "22" }
                                ),
                                new Text("Locations: ")
                            ));


                        body.AppendChild(locationParagraph);

                        Table locationtable = new Table();

                        // Define table properties
                        TableProperties locationtableProperties = new TableProperties(
                            new TableBorders(
                                new TopBorder() { Val = BorderValues.Nil },
                                new BottomBorder() { Val = BorderValues.Nil },
                                new LeftBorder() { Val = BorderValues.Nil },
                                new RightBorder() { Val = BorderValues.Nil },
                                new InsideHorizontalBorder() { Val = BorderValues.Nil },
                                new InsideVerticalBorder() { Val = BorderValues.Nil }
                            )
                        //new TableIndentation() { Width = 500, Type = TableWidthUnitValues.Dxa } // Add left margin (500 twips = 0.25 inches)
                        );
                        locationtable.AppendChild(locationtableProperties);

                        // Create the header row
                        TableRow headerRow = new TableRow();
                        headerRow.Append(
                            CreateTableCell("Loc#", false, "1200"),
                            CreateTableCell("Address", false, "5000"),
                            CreateTableCell("City", false, "2600"),
                            CreateTableCell("State", false, "2000")
                        );
                        locationtable.Append(headerRow);

                        // Create rows for each location
                        foreach (var loc in policyLocation)
                        {
                            TableRow row = new TableRow();
                            row.Append(
                                CreateTableCell(loc.LocationSequence.ToString(), false, "1200"),
                                CreateTableCell(loc.LocationAddress.ToString(), false, "5000"),
                                CreateTableCell(loc.LocationCity.ToString(), false, "2600"),
                                CreateTableCell(loc.LocationState, false, "2000")
                            );
                            locationtable.Append(row);
                        }

                        body.AppendChild(locationtable);
                    }
                }

                // end Location Section

                // Add Policy Mazor and def
                var groupedData = policyMajorClientData
                            .Where(x => x.PolicyId == policyId)
                            .GroupBy(x => x.MajorId)
                            .Select(majorGroup => new
                            {
                                MajorId = majorGroup.Key,
                                Columns = majorGroup.GroupBy(x => x.ColumnName)
                                                    .Select(columnGroup => new
                                                    {
                                                        ColumnName = columnGroup.Key,
                                                        Values = columnGroup.Select(v => new
                                                        {
                                                            v.RowSequence,
                                                            v.ColumnValue
                                                        }).ToList()
                                                    }).ToList()
                            }).ToList();

                body.AppendChild(new Paragraph(new Run(new Text(" "))));

                foreach (var item in groupedData)
                {
                    // Add spacing between tables for better readability
                    body.AppendChild(new Paragraph(new Run(new Text(" "))));

                    var majorTable = new Table();

                    // Ensure table width is consistent
                    TableProperties tableProperties = new TableProperties(
                        new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }
                    );
                    majorTable.AppendChild(tableProperties);

                    TableRow headerRow = new TableRow();

                    // Create header row with bold and underline formatting
                    int columnCount = item.Columns.Count; // Get the total number of columns

                    foreach (var col in item.Columns)
                    {
                        RunProperties runProperties = new RunProperties();
                        runProperties.Append(new Bold());
                        runProperties.Append(new Underline() { Val = UnderlineValues.Single });

                        Run headerRun = new Run(new Text(col.ColumnName));
                        headerRun.PrependChild(runProperties);

                        TableCellProperties cellProperties = new TableCellProperties();
                        cellProperties.Append(new TableCellWidth() { Width = $"{-150 / columnCount}", Type = TableWidthUnitValues.Pct });

                        TableCell cell = new TableCell(new Paragraph(headerRun))
                        {
                            TableCellProperties = cellProperties
                        };

                        headerRow.Append(cell);
                    }
                    majorTable.Append(headerRow);

                    int rowCount = item.Columns.FirstOrDefault()?.Values.Count ?? 0;

                    for (int i = 0; i < rowCount; i++)
                    {
                        TableRow dataRow = new TableRow();

                        foreach (var col in item.Columns)
                        {
                            var cellValue = col.Values.ElementAtOrDefault(i)?.ColumnValue ?? string.Empty;

                            Run dataRun = new Run(new Text(cellValue == "-1" ? "Yes" : (cellValue == "0" ? "No" : cellValue)));
                            TableCellProperties cellProperties = new TableCellProperties();
                            cellProperties.Append(new TableCellWidth() { Width = $"{-150 / columnCount}", Type = TableWidthUnitValues.Pct });

                            // Apply NoWrap **only if the table has multiple columns**
                            //if (columnCount > 1)
                            //{
                            //    cellProperties.Append(new NoWrap());
                            //}
                            ParagraphProperties paraProperties = new ParagraphProperties();
                            paraProperties.Append(new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto });

                            Paragraph dataParagraph = new Paragraph(paraProperties, dataRun);

                            TableCell dataCell = new TableCell(dataParagraph)
                            {
                                TableCellProperties = cellProperties
                            };

                            dataRow.Append(dataCell);
                        }

                        majorTable.Append(dataRow);
                    }

                    body.AppendChild(majorTable);
                }

            }

            // Page break after the section
            //body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }


        private async Task<List<PolicyMajorData>> GetPolicyMajorDataAsync(int clientId)
        {
            using (var connection = new SqlConnection("Server=stockbridgesql.database.windows.net;Database=stockbridge-db;User ID=stockbridgeadmin;Password=Hosting@123;TrustServerCertificate=True"))
            {
                var parameters = new { ClientId = clientId };

                var result = await connection.QueryAsync<PolicyMajorData>(
                    "GetPolicyMajorData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.AsList();
            }
        }

        private TableCell CreateTableCell(string text, bool isBold, string width)
        {
            return new TableCell(
                new TableCellProperties(
                    new TableCellWidth() { Width = width, Type = TableWidthUnitValues.Dxa }, // Set fixed width
                    new NoWrap() // Prevent text wrapping
                ),
                new Paragraph(
                    new ParagraphProperties(
                        new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto } // Reduce space between lines
                    ),
                    new Run(
                        new RunProperties(isBold ? new Bold() : null),
                        new Text(text) { Space = SpaceProcessingModeValues.Preserve } // Preserve spaces
                    )
                )
            );
        }

        private void AddPagination(MainDocumentPart mainPart, string clientName, string templatePrinciple, string policyNo)
        {
            FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
            Footer footer = new Footer();

            // Top border line
            Paragraph lineParagraph = new Paragraph(
                new ParagraphProperties(
                    new ParagraphBorders(
                        new BottomBorder() { Val = BorderValues.Single, Size = 6 } // Thin border
                    )
                )
            );
            footer.Append(lineParagraph);

            // First line - Company and Policy Number
            Table table1 = new Table(
                new TableProperties(
                    new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.None },
                        new BottomBorder() { Val = BorderValues.None },
                        new LeftBorder() { Val = BorderValues.None },
                        new RightBorder() { Val = BorderValues.None },
                        new InsideHorizontalBorder() { Val = BorderValues.None },
                        new InsideVerticalBorder() { Val = BorderValues.None }
                    )
                ),
                new TableRow(
                    new TableCell(
                        new Paragraph(new Run(new Text(clientName))),
                        new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "50" })
                    ),
                    new TableCell(
                        new Paragraph(
                            new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                            new Run(new Text($"Policy Number:{policyNo}"))
                        ),
                        new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "50" })
                    )
                )
            );
            footer.Append(table1);

            // Second line - Description and Date + Page Number
            Table table2 = new Table(
                new TableProperties(
                    new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.None },
                        new BottomBorder() { Val = BorderValues.None },
                        new LeftBorder() { Val = BorderValues.None },
                        new RightBorder() { Val = BorderValues.None },
                        new InsideHorizontalBorder() { Val = BorderValues.None },
                        new InsideVerticalBorder() { Val = BorderValues.None }
                    )
                ),
                new TableRow(
                    new TableCell(
                        new Paragraph(new Run(new Text(templatePrinciple))),
                        new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "50" })
                    ),
                    new TableCell(
                        new Paragraph(
                            new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                            new Run(new Text($"Date: {DateTime.UtcNow.ToString("MM/dd/yyyy")}  Page ")),
                            new Run(new SimpleField() { Instruction = "PAGE" })
                        ),
                        new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "50" })
                    )
                )
            );
            footer.Append(table2);

            footerPart.Footer = footer;
            footerPart.Footer.Save();

            SectionProperties sectionProperties = mainPart.Document.Body.Elements<SectionProperties>().LastOrDefault();
            if (sectionProperties == null)
            {
                sectionProperties = new SectionProperties();
                mainPart.Document.Body.Append(sectionProperties);
            }

            FooterReference footerReference = new FooterReference()
            {
                Type = HeaderFooterValues.Default,
                Id = mainPart.GetIdOfPart(footerPart)
            };
            sectionProperties.Append(footerReference);
        }


    }

    public class PolicyMajorData
    {
        public int PolicyId { get; set; }
        public int MajorId { get; set; }
        public int ColumnDefId { get; set; }
        public string ColumnName { get; set; }
        public int RowSequence { get; set; }
        public string ColumnValue { get; set; }
    }
}
