using System.Data;
using System.Globalization;
using Dapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using stockbridge_api.Services;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;


namespace stockbridge_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummaryReportController : ControllerBase
    {
        private readonly StockbridgeContext _context;
        List<int> policyIds = new List<int>();
        private readonly AddLogoService _addLogoService;


        public SummaryReportController(StockbridgeContext context, AddLogoService addLogoService)
        {
            _context = context;
            _addLogoService = addLogoService;
        }

        [HttpPost("generate-report")]
        public async Task<IActionResult> GenerateReport(SummaryReportRequest reportRequest)
        {
            try
            {
                byte[] fileBytes = await GenerateWordDocumentAsync(reportRequest.ClientId,reportRequest.PolicyIds);

                //System.IO.File.WriteAllBytes("Generated_Report_Debug.docx", fileBytes);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Generated_Report.docx");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GenerateWordDocumentAsync(int clientId, List<int> policyIds)
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

                        var client = await _context.Clients.FirstOrDefaultAsync(x => x.ClientId == clientId);
                        var clientName = client?.CompanyName ?? "";

                        // Add Cover page 
                        AddCoverPage(body, mainPart, clientName);

                        // Add styles
                        AddStylesPartToDocument(mainPart);

                        //AddTitlePage(body);
                        // Add TOC placeholder
                        AddTableOfContents(mainPart);

                        // Add sections
                        var policies = await _context.Policies
                            .Where(x => policyIds.Contains(x.PolicyId) && x.ClientId == clientId && !x.Expired)
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
                            .ToListAsync();

                        var clientEntity = await _context.PolicyEntities
                            .Where(x => x.ClientId == clientId && x.NamedInsured)
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
                            .Where(x => x.ClientId == clientId)
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

                        var policyMajorClientData = await GetPolicyMajorDataAsync(clientId);

                        string[] sections = await GetTableOfContent(clientId);
                        foreach (string section in sections)
                        {
                            if (section.Contains(":"))
                            {
                                string[] parts = section.Split(':');
                                string policyId = parts[0];
                                string policyTitle = parts[1];
                                AddSection(body, section, policies.ToArray(), int.Parse(policyId),
                                    clientEntity.ToArray(), policyLocations.ToArray(), clientId, policymajors, majorColDefs, minorColDefs, policyMajorClientData);
                            }
                            else
                            {
                                AddSection(body, section, policies.ToArray(), null,
                                    clientEntity.ToArray(), policyLocations.ToArray(), clientId, policymajors, majorColDefs, minorColDefs, policyMajorClientData);
                            }
                        }

                        // Ensure TOC updates when opening the document
                        AddUpdateFieldsOnOpen(mainPart);
                        AddPagination(mainPart);
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

        private void AddCoverPage(Body body, MainDocumentPart mainPart, string clientName)
        {
            // "CLIENT" Title
            Paragraph clientParagraph = new Paragraph(new Run(new Text($"{clientName}")));
            ApplyParagraphStyling(clientParagraph, JustificationValues.Center, "58", true, "008000"); // Green color

            // Separator Line
            Paragraph separatorParagraph = new Paragraph(new Run(new Text("_____________________________________________________________________________")));
            ApplyParagraphStyling(separatorParagraph, JustificationValues.Center, "18", false, "000000");

            // "INSURANCE SUMMARY" Title
            Paragraph insuranceSummaryParagraph = new Paragraph(new Run(new Text("Insurance Summary")));
            ApplyParagraphStyling(insuranceSummaryParagraph, JustificationValues.Center, "42", true, "008000");

            // Empty space (to position the content in the middle)
            AddEmptyLines(body, 6);

            // Add Logo
            Paragraph logoParagraph = _addLogoService.AddImageToDocument(mainPart, "wwwroot/images/StockbridgeLogo.jpg");


            Paragraph addressParagraph = new Paragraph(
                new Run(new Text("60 Cutter Mill Road – Suite 500")) { RunProperties = new RunProperties(new FontSize() { Val = "24" }) },
                new Run(new Break()),  // Line break
                new Run(new Text("Great Neck, NY 11021")) { RunProperties = new RunProperties(new FontSize() { Val = "24" }) }
                );
            ApplyParagraphStyling(addressParagraph, JustificationValues.Left, "14", false, "000000");

            // Contact Information
            Paragraph contactParagraph = new Paragraph(
                new Run(new Text("BY:")) { RunProperties = new RunProperties(new FontSize() { Val = "24" }) },
                new Run(new Break()),  // Line break
                new Run(new Text("PHONE:")) { RunProperties = new RunProperties(new FontSize() { Val = "24" }) },
                new Run(new Break()),  // Line break
                new Run(new Text("EMAIL:")) { RunProperties = new RunProperties(new FontSize() { Val = "24" }) },
                new Run(new Break()),  // Line break
                new Run(new Text("DATE:")) { RunProperties = new RunProperties(new FontSize() { Val = "24" }) }
            );

            ApplyParagraphStyling(contactParagraph, JustificationValues.Left, "14", false, "000000");

            // Disclaimer
            Paragraph disclaimerParagraph = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Left },
                    new SpacingBetweenLines()
                    {
                        Before = "0",
                        After = "0",
                        LineRule = LineSpacingRuleValues.Auto
                    }
                ),
                new Run(
                    new RunProperties(new FontSize() { Val = "20" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
                    new Text("THIS SUMMARY IS AN OUTLINE OF THE COVERAGE. ACTUAL POLICIES SHOULD BE REVIEWED FOR ACCURACY AS TO THE FULL TERMS, CONDITIONS, EXCLUSIONS AND LIMITATIONS")
                )
            );

            // Append all elements to the body
            body.Append(clientParagraph);
            body.Append(separatorParagraph);
            body.Append(insuranceSummaryParagraph);
            AddEmptyLines(body, 9); // Adjust space before footer
            body.Append(logoParagraph); // Append logo to the document body
            body.Append(addressParagraph);
            body.Append(contactParagraph);
            body.Append(disclaimerParagraph);

            // Add a page break for the next page
            body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
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


        private async Task<string[]> GetTableOfContent(int clientId)
        {
            string[] sections = {
                        "Legal Entity Listing",
                        "Location Listing",
                        "Expiration/Premium Summary"
                    };

            var policies = await _context.Policies
                .Where(x => x.ClientId == clientId && !x.Expired)
                .OrderBy(x => x.PrintSequence)
                .Select(x => $"{x.PolicyId}:{x.PolicyTitle}:{x.Principal.Description}")
                .ToListAsync();

            return sections.Concat(policies).ToArray();
        }

        static void AddTitlePage(Body body)
        {
            // Title
            Paragraph titleParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new FontSize() { Val = "46" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
                    new Text("Ditmas Management Corporation")
                )
            );

            // Subtitle
            Paragraph subtitleParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new FontSize() { Val = "38" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
                    new Text("Insurance Summary")
                )
            );

            // Disclaimer
            Paragraph disclaimerParagraph = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Left },
                    new SpacingBetweenLines() { After = "200" } // Add some spacing for better readability
                ),
                new Run(
                    new RunProperties(new FontSize() { Val = "20" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
                    new Text("THIS SUMMARY IS AN OUTLINE OF THE COVERAGE. ACTUAL POLICIES SHOULD BE REVIEWED FOR ACCURACY AS TO THE FULL TERMS, CONDITIONS, EXCLUSIONS AND LIMITATIONS")
                )
            );



            // Add elements to the body
            for (int i = 0; i < 7; i++)
            {
                body.AppendChild(new Paragraph(new Run(new Text(" "))));
            }
            body.AppendChild(titleParagraph);
            body.AppendChild(subtitleParagraph);
            for (int i = 0; i < 13; i++)
            {
                body.AppendChild(new Paragraph(new Run(new Text(" "))));
            }
            body.AppendChild(disclaimerParagraph);

            // Footer with horizontal line and footer text
            AddFooter(body);

            // Page break
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
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

        static void AddTableOfContents(MainDocumentPart mainPart)
        {
            var body = mainPart.Document.Body;

            // TOC Title
            Paragraph tocTitle = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Left },
                    new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto } // Adjusted line spacing
                ),
                new Run(
                    new RunProperties(
                        new Bold(),
                        new RunFonts() { Ascii = "Arial" }, // Set font to Arial
                        new FontSize() { Val = "28" } // Set font size to 36px (72 half-points)
                    ),
                    new Text("Table of Contents")
                )
            );

            body.AppendChild(tocTitle);

            for (int i = 0; i < 1; i++)
            {
                body.Append(new Paragraph(new Run(new Text(" "))));
            }

            // TOC field
            Paragraph tocField = new Paragraph(
                 new ParagraphProperties(
                    new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto } // Reduce space between lines
                ),
                new Run(new FieldChar() { FieldCharType = FieldCharValues.Begin }),
                new Run(new FieldCode(" TOC \\o \"1-3\" \\h \\z \\u ")),
                new Run(new FieldChar() { FieldCharType = FieldCharValues.Separate }),
                new Run(new FieldChar() { FieldCharType = FieldCharValues.End })
            );
            body.AppendChild(tocField);

            // Page break after TOC
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
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

        private TableCell CreateTableCell(string text, bool isBold, string width, bool isPercent)
        {
            return new TableCell(
                new TableCellProperties(
                    new TableCellWidth()
                    {
                        Width = width,
                        Type = isPercent ? TableWidthUnitValues.Pct : TableWidthUnitValues.Dxa
                    },
                    new NoWrap()
                ),
                new Paragraph(
                    new ParagraphProperties(
                        new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto }
                    ),
                    new Run(
                        new RunProperties(isBold ? new Bold() : null),
                        new Text(text) { Space = SpaceProcessingModeValues.Preserve }
                    )
                )
            );
        }

        private async void AddSection(Body body, string sectionTitle, dynamic[] policiesData,
            int? policyId, dynamic[] clientEntities, dynamic[] policyLocations, int clientId, IQueryable<PolicyMajor> policyMajorData,
            IQueryable<PolicyMajorColDef> policyMajorColDefData, IQueryable<PolicyMinorDef> policyminorDefs, List<PolicyMajorData> policyMajorClientData)
        {
            string newTitle = "";
            if (sectionTitle.Contains(":"))
            {
                string[] parts = sectionTitle.Split(':');
                sectionTitle = parts[1];
                newTitle = $"{parts[2]} --- {sectionTitle}";
            }
            else
            {
                newTitle = sectionTitle;
            }

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
                        new FontSize() { Val = "28" }, // Size 40px (80 half-points)
                        new Color() { Val = "265316" }
                        //new Color() { Val = "000000" }, // Black text color
                        //new Underline() { Val = UnderlineValues.Single } // Add underline
                    ),
                    new Text(newTitle)
                )
            );

            // Append the heading and content to the document
            body.AppendChild(heading);

            // Create the content
            Paragraph content = new Paragraph();
            if (sectionTitle == "Legal Entity Listing")
            {
                var entities = _context.ClientEntities
                    .Where(x => x.ClientId == clientId)
                    .Select(x => new { x.Sequence, x.Name })
                    .OrderBy(x => x.Name)
                    .ToList();

                if (entities.Any())
                {
                    AddEmptyLines(body, 1);

                    Table table = new Table();

                    // Define table properties
                    TableProperties entityTblProperties = new TableProperties(
                        new TableBorders(
                            new TopBorder() { Val = BorderValues.Nil },
                            new BottomBorder() { Val = BorderValues.Nil },
                            new LeftBorder() { Val = BorderValues.Nil },
                            new RightBorder() { Val = BorderValues.Nil },
                            new InsideHorizontalBorder() { Val = BorderValues.Nil },
                            new InsideVerticalBorder() { Val = BorderValues.Nil }
                        )
                    );
                    table.AppendChild(entityTblProperties);

                    // Create the header row
                    TableRow headerRow = new TableRow();
                    headerRow.Append(
                        CreateTableCell("Entity #", true, "1500"),
                        CreateTableCell("Entity Name", true, "8000")
                    );
                    table.Append(headerRow);

                    // Create rows for each location
                    foreach (var e in entities)
                    {
                        TableRow row = new TableRow();
                        row.Append(
                            CreateTableCell(e.Sequence.ToString(), false, "1500"),
                            CreateTableCell(e.Name, false, "8000")
                        );
                        table.Append(row);
                    }

                    body.AppendChild(table);
                }
            }
            else if (sectionTitle == "Location Listing")
            {
                var locations = _context.ClientLocations
                    .Where(x => x.ClientId == clientId)
                    .Select(x => new { x.Sequence, x.Address1, x.City, x.State })
                    .OrderBy(x => x.State)
                    .ToList();

                if (locations.Any())
                {
                    AddEmptyLines(body, 1);

                    Table table = new Table();

                    // Define table properties
                    TableProperties tblProperties = new TableProperties(
                        new TableBorders(
                            new TopBorder() { Val = BorderValues.Nil },
                            new BottomBorder() { Val = BorderValues.Nil },
                            new LeftBorder() { Val = BorderValues.Nil },
                            new RightBorder() { Val = BorderValues.Nil },
                            new InsideHorizontalBorder() { Val = BorderValues.Nil },
                            new InsideVerticalBorder() { Val = BorderValues.Nil }
                        )
                    );
                    table.AppendChild(tblProperties);

                    // Create the header row
                    TableRow headerRow = new TableRow();
                    headerRow.Append(
                        CreateTableCell("Loc #", true, "2000"),
                        CreateTableCell("Address", true, "5000"),
                        CreateTableCell("City", true, "2500"),
                        CreateTableCell("State", true, "1200")
                    );
                    table.Append(headerRow);


                    // Create rows for each location
                    foreach (var loc in locations)
                    {
                        TableRow row = new TableRow();
                        row.Append(
                            CreateTableCell(loc.Sequence.ToString(), false, "2000"),
                            CreateTableCell(loc.Address1, false, "5000"),
                            CreateTableCell(loc.City, false, "2500"),
                            CreateTableCell(loc.State, false, "1200")
                        );
                        table.Append(row);
                    }

                    body.AppendChild(table);
                }
            }
            else if (sectionTitle == "Expiration/Premium Summary")
            {
                var policies = _context.Policies
                   .Where(x => x.ClientId == clientId && !x.Expired)
                   .OrderBy(x => x.PrintSequence)
                   .Select(x => new
                   {
                       x.PolicyNo,
                       x.PolicyTitle,
                       x.ExpirationDate,
                       x.AnualPremium,
                       TemplateDescription = _context.TemplatePrincipals
                                                 .Where(td => td.PrincipalId == x.PrincipalId)
                                                 .Select(td => td.Description)
                                                 .FirstOrDefault()
                   })
                   .ToList();

                if (policies.Any())
                {
                    AddEmptyLines(body, 1);
                    Table table = new Table();

                    // Define table properties with black borders
                    TableProperties tblProperties = new TableProperties(
                        new TableBorders(
                            new TopBorder() { Val = BorderValues.Single, Size = 8, Color = "000000" },
                            new BottomBorder() { Val = BorderValues.Single, Size = 8, Color = "000000" },
                            new LeftBorder() { Val = BorderValues.Single, Size = 8, Color = "000000" },
                            new RightBorder() { Val = BorderValues.Single, Size = 8, Color = "000000" },
                            new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 8, Color = "000000" },
                            new InsideVerticalBorder() { Val = BorderValues.Single, Size = 8, Color = "000000" }
                        )
                    );
                    table.AppendChild(tblProperties);

                    //// Create the header row
                    TableRow headerRow = new TableRow();
                    headerRow.Append(
                        CreateTableCell("Policy Number", true, "2200", "20"),
                        CreateTableCell("Type Of Insurance", true, "3000", "20"),
                        CreateTableCell("Policy Title", true, "2500", "20"),
                        CreateTableCell("Expiration", true, "2000", "20"),
                        CreateTableCell("Annual Premium", true, "2500", "20")
                    );
                    table.Append(headerRow);

                    double grandTotalAmount = 0;

                    // Create rows for each location
                    foreach (var policy in policies)
                    {
                        TableRow row = new TableRow();
                        row.Append(
                            CreateTableCell(policy.PolicyNo.ToString(), false, "2200", "20"),
                            CreateTableCell(policy.TemplateDescription ?? "", false, "3000", "20"),
                            CreateTableCell(policy.PolicyTitle, false, "2500", "20"),
                            CreateTableCell(policy.ExpirationDate?.ToString("MM/dd/yyyy").Replace('-', '/'), false, "2000", "20"),
                            CreateTableCell(policy.AnualPremium?.ToString("C0", CultureInfo.GetCultureInfo("en-US")) ?? string.Empty, false, "2500", "20")
                        );
                        table.Append(row);

                        grandTotalAmount += policy.AnualPremium ?? 0;
                    }

                    TableRow grandTotal = new TableRow();
                    grandTotal.Append(
                        CreateTableCell(" ", false, "2200", "20"),
                        CreateTableCell(" ", false, "3000", "20"),
                        CreateTableCell(" ", false, "2500", "20"),
                        CreateTableCell("Grand Total : ", true, "2000", "23"),
                        CreateTableCell(grandTotalAmount.ToString("C0", CultureInfo.GetCultureInfo("en-US")) ?? string.Empty, true, "2500", "23")
                    );
                    table.Append(grandTotal);

                    body.AppendChild(table);
                }
            }
            else
            {
                // Policy Summary Detail Section
                var policy = policiesData.FirstOrDefault(x => x.Policy.PolicyTitle == sectionTitle && x.Policy.PolicyId == policyId);

                if (policy != null)
                {
                    Table table = new Table();

                    // Define table properties
                    TableProperties tblProperties = new TableProperties(
                        new TableWidth { Width = "11000", Type = TableWidthUnitValues.Dxa }, // Optional: set width
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
                        CreateRow("Carrier", policy.CarrierName, "Last Updated", ""),
                        CreateRow("Policy Number", policy.Policy.PolicyNo, "", ""),
                        CreateRow("Inception", policy.Policy.InceptionDate?.ToString("MM/dd/yyyy") ?? "", "Expiration", policy.Policy.ExpirationDate?.ToString("MM/dd/yyyy") ?? ""),
                        CreateRow("Annual Premium", policy.Policy.AnualPremium?.ToString("C0", CultureInfo.GetCultureInfo("en-US")) ?? "", "Auditable", policy.Policy.Audit ? "Yes" : "No")
                    );

                    // Append the table to the document body
                    body.AppendChild(table);
                    // add empty row for spacing
                    body.AppendChild(new Paragraph(new Run(new Text(""))));

                    // add policy comment
                    //content = new Paragraph(
                    //new Run(
                    //    new Text(""), new TabChar(),  // Label with a tab space
                    //    new Text(policy.Policy.PolicyComment ?? "")  // Ensure no null issues
                    //));
                    //body.AppendChild(content);

                    // Check if PolicyComment is null or empty before processing
                    if (!string.IsNullOrWhiteSpace(policy.Policy.PolicyComment))
                    {
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
                    else
                    {
                        // Optional: Handle the case where PolicyComment is null or empty
                        var emptyMessage = new Paragraph(
                            new Run(
                                new Text(" ") // Fallback text
                            )
                        );
                        body.AppendChild(emptyMessage);
                    }

                    // end policy Comment

                    // Add Name insured
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
                    var entities = clientEntities.Where(x => x.PolicyEntity.PolicyId == policyId).ToList();
                    if (entities.Any())
                    {
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
                    // end Name Insured

                    // Add Location Section
                    if (!policy.Policy.SuppressLocations)
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
                        var policyLocation = policyLocations
                                            .Where(x => x.PolicyEntity.PolicyId == policyId)
                                            .ToList();
                        if (policyLocation.Any())
                        {
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
                    var grouped_Data = policyMajorClientData
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

                    var groupedData = grouped_Data
                                        .Select(major =>
                                        {
                                            var firstColumn = major.Columns.FirstOrDefault();

                                            // Get valid RowSequences from first column (non-empty values only)
                                            var validSequences = firstColumn?.Values
                                                .Where(v => !string.IsNullOrEmpty(v.ColumnValue))
                                                .Select(v => v.RowSequence)
                                                .ToHashSet() ?? new HashSet<int>();

                                            // Filter all columns based on those sequences
                                            var trimmedColumns = major.Columns.Select(col => new
                                            {
                                                ColumnName = col.ColumnName,
                                                Values = col.Values
                                                    .Where(v => validSequences.Contains(v.RowSequence))
                                                    .ToList()
                                            }).ToList();

                                            return new
                                            {
                                                MajorId = major.MajorId,
                                                Columns = trimmedColumns
                                            };
                                        })
                                        .ToList();


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
                                //int columWidth = 20;
                                //bool isYesOrNo = (cellValue == "Yes" || cellValue == "No") == true;
                                //cellProperties.Append(new TableCellWidth() { Width = $"{-150 / columnCount}", Type = TableWidthUnitValues.Pct });


                                bool isYesOrNo = (cellValue == "Yes" || cellValue == "No") == true ? true : false;
                                string columWidth = (isYesOrNo == true && columnCount>2) == true ? "-10" : $"{-140 / columnCount}";
                                cellProperties.Append(new TableCellWidth() { Width = columWidth, Type = TableWidthUnitValues.Pct });
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

            }

            // Page break after the section
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
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

        private TableCell CreateTableCell(string text, bool isBold, string width, string fontSize)
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
                        new RunProperties(
                            isBold ? new Bold() : null,
                            new FontSize() { Val = fontSize } // Set font size
                        ),
                        new Text(text) { Space = SpaceProcessingModeValues.Preserve } // Preserve spaces
                    )
                )
            );
        }


        private void AddPagination(MainDocumentPart mainPart)
        {
            FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
            Footer footer = new Footer();

            Paragraph lineParagraph = new Paragraph();
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            ParagraphBorders paragraphBorders = new ParagraphBorders();
            paragraphBorders.BottomBorder = new BottomBorder() { Val = BorderValues.Single, Size = 12 }; // Adjust size if needed
            paragraphProperties.Append(paragraphBorders);
            lineParagraph.Append(paragraphProperties);

            Paragraph paragraph = new Paragraph(new Run(new Text("Page- ")));
            SimpleField pageNumberField = new SimpleField() { Instruction = "PAGE" };
            paragraph.Append(new Run(pageNumberField));

            footer.Append(lineParagraph);
            footer.Append(paragraph);
            footerPart.Footer = footer;
            footerPart.Footer.Save();

            SectionProperties sectionProperties = mainPart.Document.Body.Elements<SectionProperties>().LastOrDefault();
            if (sectionProperties == null)
            {
                sectionProperties = new SectionProperties();
                mainPart.Document.Body.Append(sectionProperties);
            }

            FooterReference footerReference = new FooterReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) };
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

