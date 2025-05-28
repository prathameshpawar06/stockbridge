using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stockbridge_DAL.domainModels;

namespace stockbridge_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly StockbridgeContext _context;

        public ReportController(StockbridgeContext context)
        {
            _context = context;
        }

        [HttpPost("generate-report")]
        public async Task<IActionResult> GenerateReport()
        {
            try
            {
                byte[] fileBytes = await GenerateWordDocumentAsync();

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Generated_Report.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GenerateWordDocumentAsync()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // Add styles
                    AddStylesPartToDocument(mainPart);

                    // Add the designed title page
                    AddTitlePage(body);

                    // Add TOC placeholder
                    AddTableOfContents(mainPart);

                    // Add sections
                    string[] sections = await GetTableOfContent();
                    foreach (string section in sections)
                    {
                        AddSection(body, section);
                    }

                    // Ensure TOC updates when opening the document
                    AddUpdateFieldsOnOpen(mainPart);

                    mainPart.Document.Save();
                }

                return memoryStream.ToArray();
            }
        }

        private async Task<string[]> GetTableOfContent()
        {
            string[] sections = {
                        "Legal Entity Listing",
                        "Location Listing",
                        "Expiration/Premium Summary"
                    };

            var policies = await _context.Policies
                .Where(x => x.ClientId == 410 && !x.Expired)
                .OrderBy(x => x.PrintSequence)
                .Select(x => x.PolicyTitle)
                .ToListAsync();

            return sections.Concat(policies).ToArray();
        }

        static void AddTitlePage(Body body)
        {
            // Title
            Paragraph titleParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new FontSize() { Val = "48" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
                    new Text("Ditmas Management Corporation")
                )
            );

            // Subtitle
            Paragraph subtitleParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new FontSize() { Val = "40" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
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
                    new RunProperties(new FontSize() { Val = "22" }, new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" }),
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
                    new RunProperties(new FontSize() { Val = "20" }),
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
                        new FontSize() { Val = "20" } // 14pt
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
                    new Justification() { Val = JustificationValues.Left }
                ),
                new Run(
                    new RunProperties(
                        new Bold(),
                        new RunFonts() { Ascii = "Arial" }, // Set font to Arial
                        new FontSize() { Val = "36" } // Set font size to 36px (72 half-points)
                    ),
                    new Text("Table of Contents")
                )
            );

            body.AppendChild(tocTitle);

            // TOC field
            Paragraph tocField = new Paragraph(
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

        private void AddSection(Body body, string sectionTitle)
        {
            // Create the heading
            Paragraph heading = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Left },
                    new ParagraphStyleId() { Val = "Heading1" }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Aptos Display" },
                        new FontSize() { Val = "40" }, // Size 40px (80 half-points)
                        new Color() { Val = "4e778a" }
                    ),
                    new Text(sectionTitle)
                )
            );



            // Append the heading and content to the document
            body.AppendChild(heading);

            // Create the content
            Paragraph content = new Paragraph();
            if (sectionTitle == "Legal Entity Listing")
            {
                var entities = _context.ClientEntities
                    .Where(x => x.ClientId == 410)
                    .Select(x => new { x.Sequence, x.Name })
                    .ToList();

                if (entities.Any())
                {
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
                        CreateTableCell("Entity #", true, "1200"),
                        CreateTableCell("Entity Name", true, "5000")
                    );
                    table.Append(headerRow);

                    // Create rows for each location
                    foreach (var e in entities)
                    {
                        TableRow row = new TableRow();
                        row.Append(
                            CreateTableCell(e.Sequence.ToString(), false, "1200"),
                            CreateTableCell(e.Name, false, "5000")
                        );
                        table.Append(row);
                    }

                    body.AppendChild(table);
                }

                //string entityHeaderText = "Ent #      Entity Name";
                //content.AppendChild(new Run(new Text(entityHeaderText)));
                //content.AppendChild(new Run(new Break()));

                //foreach (var entity in entities)
                //{
                //    string entityText = entity.Sequence + "      " + entity.Name;
                //    content.AppendChild(new Run(new Text(entityText)));
                //    content.AppendChild(new Run(new Break())); // This adds the line break
                //}
                //body.AppendChild(content);

            }
            else if (sectionTitle == "Location Listing")
            {
                var locations = _context.ClientLocations
                    .Where(x => x.ClientId == 410)
                    .Select(x => new { x.Sequence, x.Address1, x.City, x.State })
                    .ToList();

                if (locations.Any())
                {
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
                        CreateTableCell("Loc #", true, "1200"),
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
                            CreateTableCell(loc.Sequence.ToString(), false, "1200"),
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
                    .Where(x => x.ClientId == 410 && !x.Expired)
                    .OrderBy(x => x.PrintSequence)
                    .Select(x => new { x.PolicyNo, x.PolicyTitle, x.ExpirationDate, x.AnualPremium })
                    .ToList();

                if (policies.Any())
                {
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

                    //// Create the header row
                    TableRow headerRow = new TableRow();
                    headerRow.Append(
                        CreateTableCell("Policy Number", true, "2200"),
                        CreateTableCell("Type Of Insurance", true, "3000"),
                        CreateTableCell("Policy Title", true, "2500"),
                        CreateTableCell("Expiration", true, "2000"),
                        CreateTableCell("Annual Premium", true, "2500")
                    );
                    table.Append(headerRow);

                    // Create rows for each location
                    foreach (var policy in policies)
                    {
                        TableRow row = new TableRow();
                        row.Append(
                            CreateTableCell(policy.PolicyNo.ToString(), false, "2200"),
                            CreateTableCell("", false, "3000"),
                            CreateTableCell(policy.PolicyTitle, false, "2500"),
                            CreateTableCell(policy.ExpirationDate?.ToString("MM/dd/yyyy").Replace('-', '/'), false, "2000"),
                            CreateTableCell("$" + policy.AnualPremium.ToString(), false, "2500")
                        );
                        table.Append(row);
                    }

                    body.AppendChild(table);
                }
            }
            else
            {
                // Dummy text for large content (as in your original code)
                content = new Paragraph(
                    new Run(new Text(new string('X', 3000))) // Dummy text
                );
                body.AppendChild(content);

            }


            // Page break after the section
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }

        private TableCell CreateTableCell(string text, bool isBold, string width)
        {
            return new TableCell(
                new TableCellProperties(
                    new TableCellWidth() { Width = width, Type = TableWidthUnitValues.Dxa }, // Set fixed width
                    new NoWrap() // Prevent text wrapping
                ),
                new Paragraph(
                    new Run(
                        new RunProperties(isBold ? new Bold() : null),
                        new Text(text) { Space = SpaceProcessingModeValues.Preserve } // Preserve spaces
                    )
                )
            );
        }

        private TableCell CreateCarrierBrokerTableCell(string text, bool isBold, string width, bool isRightAligned = false, bool isUnderlined = false)
        {
            return new TableCell(
                new TableCellProperties(
                    new TableCellWidth() { Width = width, Type = TableWidthUnitValues.Dxa }, // Set fixed width
                    new NoWrap() // Prevent text wrapping
                ),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = isRightAligned ? JustificationValues.Right : JustificationValues.Center } // Align text to right or center
                    ),
                    new Run(
                        new RunProperties(
                            isBold ? new Bold() : null,
                            isUnderlined ? new Underline() { Val = UnderlineValues.Single } : null // Conditionally add underline
                        ),
                        new Text(text) { Space = SpaceProcessingModeValues.Preserve } // Preserve spaces
                    )
                )
            );
        }

        [HttpPost("GenerateBrokerReport/{brokerId}")]
        public async Task<IActionResult> GenerateBrokerReport(int brokerId)
        {
            try
            {
                byte[] fileBytes = await GenerateBrokerWordDocumentAsync(brokerId);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Broker_Report.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GenerateBrokerWordDocumentAsync(int brokerId)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // Add the header to the document
                    await AddBrokerHeader(mainPart, brokerId);

                    await AddBrokerSection(body, brokerId);

                    // Ensure TOC updates when opening the document
                    AddUpdateFieldsOnOpen(mainPart);

                    //AddFooterToAllPages(mainPart);

                    mainPart.Document.Save();
                }

                return memoryStream.ToArray();
            }
        }

        private async Task AddBrokerSection(Body body, int brokerId)
        {
            // Fetch policies associated with the broker
            var policies = await _context.Policies
                .Where(x => x.BrokerId == brokerId)
                .Select(x => new { x.PolicyNo, x.PolicyType, x.PolicyTitle, x.ExpirationDate, x.AnualPremium })
                .OrderBy(x => x.PolicyNo)
                .ToListAsync();

            if (policies.Any())
            {
                Table table = new Table();

                // Define table properties
                TableProperties tableProperties = new TableProperties(
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.Nil },
                        new BottomBorder() { Val = BorderValues.Nil },
                        new LeftBorder() { Val = BorderValues.Nil },
                        new RightBorder() { Val = BorderValues.Nil },
                        new InsideHorizontalBorder() { Val = BorderValues.Nil },
                        new InsideVerticalBorder() { Val = BorderValues.Nil }
                    )
                );
                table.AppendChild(tableProperties);

                // Create rows for each policy
                foreach (var policy in policies)
                {
                    TableRow policyRow = new TableRow();
                    policyRow.Append(
                        CreateCarrierBrokerTableCell(policy.PolicyNo?.ToString() ?? "N/A", false, "5000"),
                        CreateCarrierBrokerTableCell(policy.PolicyType.ToString() ?? "N/A", false, "4000"),
                        CreateCarrierBrokerTableCell(policy.PolicyTitle ?? "N/A", false, "5000"),
                        CreateCarrierBrokerTableCell(policy.ExpirationDate?.ToString("MM/dd/yyyy") ?? "N/A", false, "3100"),
                        CreateCarrierBrokerTableCell($"${policy.AnualPremium?.ToString("N2") ?? "0.00"}", false, "3100", true)
                    );
                    table.Append(policyRow);
                }

                body.AppendChild(table);
            }
            else
            {
                // If no policies found, add a message
                Paragraph noPolicies = new Paragraph(
                    new Run(new Text("No policies available for this broker."))
                );
                body.AppendChild(noPolicies);
            }

            // Add a page break after the section
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }

        private async Task AddBrokerHeader(MainDocumentPart mainPart, int brokerId)
        {
            var broker = await _context.Brokers.FindAsync(brokerId);

            if (broker == null)
            {
                return; // Exit if broker is not found
            }

            HeaderPart headerPart = mainPart.AddNewPart<HeaderPart>();
            string headerPartId = mainPart.GetIdOfPart(headerPart);

            // Create the header
            Header header = new Header();

            Paragraph headerParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Arial Black" },
                        new FontSize() { Val = "36" }, // Large font size
                        new Bold()
                    ),
                    new Text("POLICIES BY BROKER LISTING")
                )
            );

            header.AppendChild(headerParagraph);

            // Create a table with two columns (Broker Name on left, Date on right)
            Table headingTable = new Table();

            // Define table properties (no borders for cleaner look)
            TableProperties tblProperties = new TableProperties(
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Auto }
            );
            headingTable.AppendChild(tblProperties);

            // Create the row
            TableRow row = new TableRow();

            // Create Broker Name cell (Left-aligned)
            TableCell brokerCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "70%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Left }
                    ),
                    new Run(
                        new RunProperties(
                            //new RunFonts() { Ascii = "Arial Black" },
                            new FontSize() { Val = "28" } // 40px font size
                        ),
                        new Text($"Broker: {broker.Name}")
                    )
                )
            );

            // Create Date Cell (Right-aligned)
            TableCell dateCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "30%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Right }
                    ),
                    new Run(
                        new RunProperties(
                            new FontSize() { Val = "28" } // Slightly smaller font
                        ),
                        new Text(DateTime.Now.ToString("MM/dd/yy"))
                    )
                )
            );

            // Append cells to row
            row.Append(brokerCell, dateCell);

            // Append row to table
            headingTable.Append(row);

            // Append table to header
            header.AppendChild(headingTable);

            // Add a horizontal line
            Paragraph horizontalLine = new Paragraph(
                new ParagraphProperties(
                    new ParagraphBorders(
                        new BottomBorder() { Val = BorderValues.Single, Color = "000000", Size = 12 } // Black single-line border
                    )
                )
            );

            header.AppendChild(horizontalLine);

            //For table column 
            Table table = new Table();

            // Define table properties
            TableProperties tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Nil },
                    new BottomBorder() { Val = BorderValues.Nil },
                    new LeftBorder() { Val = BorderValues.Nil },
                    new RightBorder() { Val = BorderValues.Nil },
                    new InsideHorizontalBorder() { Val = BorderValues.Nil },
                    new InsideVerticalBorder() { Val = BorderValues.Nil }
                )
            );
            table.AppendChild(tableProperties);

            // Create the header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateCarrierBrokerTableCell("Policy Number", false, "5000", false, true),
                CreateCarrierBrokerTableCell("Type of Insurance", false, "4000", false, true),
                CreateCarrierBrokerTableCell("Policy Title", false, "5000", false, true),
                CreateCarrierBrokerTableCell("Expiration", false, "3100", false, true),
                CreateCarrierBrokerTableCell("Annual Premium", false, "3100", false, true)
            );
            table.Append(headerRow);
            header.AppendChild(table);

            // Set header content
            headerPart.Header = header;
            headerPart.Header.Save();

            // Apply the header to all pages
            SectionProperties sectionProperties = mainPart.Document.Body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
            HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = headerPartId };

            sectionProperties.PrependChild(headerReference);
            mainPart.Document.Body.AppendChild(sectionProperties);
        }

        //private void AddFooterToAllPages(MainDocumentPart mainPart)
        //{
        //    FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
        //    string footerPartId = mainPart.GetIdOfPart(footerPart);

        //    Footer footer = new Footer();

        //    // Add a horizontal line
        //    footer.Append(new Paragraph(
        //        new ParagraphProperties(new ParagraphBorders(new BottomBorder() { Val = BorderValues.Single, Color = "000000", Size = 12 }))
        //    ));

        //    // Create footer content
        //    Table footerTable = new Table();
        //    TableRow footerRow = new TableRow();

        //    TableCell leftCell = new TableCell(new Paragraph(new Run(new Text("Policies with: A. J. Pawar"))));
        //    leftCell.AppendChild(new TableCellProperties(new TableCellWidth() { Width = "70%", Type = TableWidthUnitValues.Dxa }));
        //    footerRow.Append(leftCell);

        //    TableCell rightCell = new TableCell(new Paragraph(new Run(new Text("Page: "))));
        //    //rightCell.AppendChild(new SimpleField() { Instruction = "PAGE" });
        //    rightCell.AppendChild(new TableCellProperties(new TableCellWidth() { Width = "30%", Type = TableWidthUnitValues.Dxa }));
        //    footerRow.Append(rightCell);

        //    footerTable.Append(footerRow);
        //    footer.AppendChild(footerTable);

        //    footerPart.Footer = footer;
        //    footerPart.Footer.Save();

        //    // Apply footer to all pages
        //    SectionProperties sectionProperties = new SectionProperties();
        //    sectionProperties.Append(new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId });

        //    var existingSectionProperties = mainPart.Document.Body.Elements<SectionProperties>().FirstOrDefault();
        //    if (existingSectionProperties != null)
        //    {
        //        mainPart.Document.Body.ReplaceChild(sectionProperties.CloneNode(true), existingSectionProperties);
        //    }
        //    else
        //    {
        //        mainPart.Document.Body.AppendChild(sectionProperties.CloneNode(true));
        //    }
        //}

        private void AddBrokerFooter__(MainDocumentPart mainPart)
        {
            FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
            string footerPartId = mainPart.GetIdOfPart(footerPart);

            Footer footer = new Footer();

            // Add a horizontal line
            Paragraph footerLine = new Paragraph(
                new ParagraphProperties(
                    new ParagraphBorders(
                        new TopBorder() { Val = BorderValues.Single, Color = "000000", Size = 12 } // Black single-line border
                    )
                )
            );
            footer.AppendChild(footerLine);

            // Create a table for footer content
            Table footerTable = new Table();

            // Define table properties (no borders for cleaner look)
            TableProperties tblProperties = new TableProperties(
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Auto }
            );
            footerTable.AppendChild(tblProperties);

            // Create a row for the footer
            TableRow footerRow = new TableRow();

            // Left side: Policies with Broker Name
            TableCell leftCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "70%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(new Justification() { Val = JustificationValues.Left }),
                    new Run(new Text("Policies with : A. J. Pawar"))
                )
            );
            footerRow.Append(leftCell);

            // Right side: Page Number
            TableCell rightCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "30%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                    new Run(new Text("Page ")),
                    new SimpleField() { Instruction = "PAGE" } // Auto-incrementing page number
                )
            );
            footerRow.Append(rightCell);

            // Append row to table
            footerTable.Append(footerRow);
            footer.AppendChild(footerTable);

            // Set footer content
            footerPart.Footer = footer;
            footerPart.Footer.Save();

            // Apply the footer to all pages
            SectionProperties sectionProperties = mainPart.Document.Body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
            FooterReference footerReference = new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId };

            sectionProperties.PrependChild(footerReference);
            mainPart.Document.Body.AppendChild(sectionProperties);
        }

        [HttpPost("GenerateCarrierReport/{carrierId}")]
        public async Task<IActionResult> GenerateCarrierReport(int carrierId)
        {
            try
            {
                byte[] fileBytes = await GenerateCarrierWordDocumentAsync(carrierId);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Carrier_Report.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GenerateCarrierWordDocumentAsync(int carrierId)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // Add the header to the document
                    await AddCarrierHeader(mainPart, carrierId);

                    await AddCarrierSection(body, carrierId);

                    // Ensure TOC updates when opening the document
                    AddUpdateFieldsOnOpen(mainPart);

                    //AddFooterToAllPages(mainPart);

                    mainPart.Document.Save();
                }

                return memoryStream.ToArray();
            }
        }

        private async Task AddCarrierSection(Body body, int carrierId)
        {
            // Fetch policies associated with the broker
            var policies = await _context.Policies
                .Where(x => x.CarrierId == carrierId)
                .Select(x => new { x.PolicyNo, x.PolicyType, x.PolicyTitle, x.ExpirationDate, x.AnualPremium })
                .OrderBy(x => x.PolicyNo)
                .ToListAsync();

            if (policies.Any())
            {
                Table table = new Table();

                // Define table properties
                TableProperties tableProperties = new TableProperties(
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.Nil },
                        new BottomBorder() { Val = BorderValues.Nil },
                        new LeftBorder() { Val = BorderValues.Nil },
                        new RightBorder() { Val = BorderValues.Nil },
                        new InsideHorizontalBorder() { Val = BorderValues.Nil },
                        new InsideVerticalBorder() { Val = BorderValues.Nil }
                    )
                );
                table.AppendChild(tableProperties);

                // Create rows for each policy
                foreach (var policy in policies)
                {
                    TableRow policyRow = new TableRow();
                    policyRow.Append(
                        CreateCarrierBrokerTableCell(policy.PolicyNo?.ToString() ?? "N/A", false, "3100"),
                        CreateCarrierBrokerTableCell(policy.PolicyType.ToString() ?? "N/A", false, "5000"),
                        CreateCarrierBrokerTableCell(policy.PolicyTitle ?? "N/A", false, "5000"),
                        CreateCarrierBrokerTableCell(policy.ExpirationDate?.ToString("MM/dd/yyyy") ?? "N/A", false, "3100"),
                        CreateCarrierBrokerTableCell($"${policy.AnualPremium?.ToString("N2") ?? "0.00"}", false, "3100", true)
                    );
                    table.Append(policyRow);
                }

                body.AppendChild(table);
            }
            else
            {
                // If no policies found, add a message
                Paragraph noPolicies = new Paragraph(
                    new Run(new Text("No policies available for this Carrier."))
                );
                body.AppendChild(noPolicies);
            }

            // Add a page break after the section
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }

        private async Task AddCarrierHeader(MainDocumentPart mainPart, int carrierId)
        {
            var carrier = await _context.Carriers.FindAsync(carrierId);

            if (carrier == null)
            {
                return; // Exit if carrier is not found
            }

            HeaderPart headerPart = mainPart.AddNewPart<HeaderPart>();
            string headerPartId = mainPart.GetIdOfPart(headerPart);

            // Create the header
            Header header = new Header();

            Paragraph headerParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Arial Black" },
                        new FontSize() { Val = "36" }, // Large font size
                        new Bold()
                    ),
                    new Text("POLICIES BY CARRIER LISTING")
                )
            );

            header.AppendChild(headerParagraph);

            // Create a table with two columns (Broker Name on left, Date on right)
            Table headingTable = new Table();

            // Define table properties (no borders for cleaner look)
            TableProperties tblProperties = new TableProperties(
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Auto }
            );
            headingTable.AppendChild(tblProperties);

            // Create the row
            TableRow row = new TableRow();

            // Create Broker Name cell (Left-aligned)
            TableCell brokerCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "70%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Left }
                    ),
                    new Run(
                        new RunProperties(
                            //new RunFonts() { Ascii = "Arial Black" },
                            new FontSize() { Val = "28" } // 40px font size
                        ),
                        new Text($"Carrier: {carrier.Name}")
                    )
                )
            );

            // Create Date Cell (Right-aligned)
            TableCell dateCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "30%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Right }
                    ),
                    new Run(
                        new RunProperties(
                            new FontSize() { Val = "28" } // Slightly smaller font
                        ),
                        new Text(DateTime.Now.ToString("MM/dd/yy"))
                    )
                )
            );

            // Append cells to row
            row.Append(brokerCell, dateCell);

            // Append row to table
            headingTable.Append(row);

            // Append table to header
            header.AppendChild(headingTable);

            // Add a horizontal line
            Paragraph horizontalLine = new Paragraph(
                new ParagraphProperties(
                    new ParagraphBorders(
                        new BottomBorder() { Val = BorderValues.Single, Color = "000000", Size = 12 } // Black single-line border
                    )
                )
            );

            header.AppendChild(horizontalLine);

            //For table column 
            Table table = new Table();

            // Define table properties
            TableProperties tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Nil },
                    new BottomBorder() { Val = BorderValues.Nil },
                    new LeftBorder() { Val = BorderValues.Nil },
                    new RightBorder() { Val = BorderValues.Nil },
                    new InsideHorizontalBorder() { Val = BorderValues.Nil },
                    new InsideVerticalBorder() { Val = BorderValues.Nil }
                )
            );
            table.AppendChild(tableProperties);

            // Create the header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateCarrierBrokerTableCell("Policy Number", false, "5000", false, true),
                CreateCarrierBrokerTableCell("Type of Insurance", false, "4000", false, true),
                CreateCarrierBrokerTableCell("Policy Title", false, "5000", false, true),
                CreateCarrierBrokerTableCell("Expiration", false, "3100", false, true),
                CreateCarrierBrokerTableCell("Annual Premium", false, "3100", false, true)
            );
            table.Append(headerRow);
            header.AppendChild(table);

            // Set header content
            headerPart.Header = header;
            headerPart.Header.Save();

            // Apply the header to all pages
            SectionProperties sectionProperties = mainPart.Document.Body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
            HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = headerPartId };

            sectionProperties.PrependChild(headerReference);
            mainPart.Document.Body.AppendChild(sectionProperties);
        }


        #region Expiration Summary Report
        [HttpGet("GenerateExpirationReport")]
        public async Task<IActionResult> GenerateExpirationReport(string startDate, string endDate, string selectedStaffIds)
        {
            try
            {
                //string[] staffIds = selectedStaffIds.Split(",");
                int[] staffIds = selectedStaffIds.Split(',')
                                 .Select(id => int.TryParse(id, out int value) ? value : 0)
                                 .Where(id => id > 0) // Remove invalid or zero values
                                 .ToArray();

                byte[] fileBytes = await GenerateExpirationWordDocumentAsync(startDate, endDate, staffIds);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "ExpirationSummary_Report.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GenerateExpirationWordDocumentAsync(string startDate, string endDate, int[] staffIds)
        {
            return await Task.Run(() =>
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
                    {
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = new Body();
                        body.Append(CreateNarrowPageLayout());

                        foreach (var staffId in staffIds)
                        {
                            AddStaffReport(body, staffId, startDate, endDate);
                        }

                        mainPart.Document.Append(body);
                        mainPart.Document.Save();

                        AddPagination(mainPart);
                    }
                    return memStream.ToArray();
                }
            });
        }

        private static SectionProperties CreateNarrowPageLayout()
        {
            return new SectionProperties(
                new PageSize() { Width = 12240, Height = 15840 }, // A4 size
                new PageMargin()
                {
                    Top = 360,    // 0.25 inch
                    Bottom = 360, // 0.25 inch
                    Left = 360,   // 0.25 inch
                    Right = 360,  // 0.25 inch
                    Header = 450,
                    Footer = 450,
                    Gutter = 0
                }
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

            Paragraph paragraph = new Paragraph(new Run(new Text("Page ")));
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

        private async Task AddExpirationSection(Body body, string startDate, string endDate, int[] staffIds)
        {
            // Initialize variables
            DateTime parsedStartDate = DateTime.MinValue;
            DateTime parsedEndDate = DateTime.MaxValue;

            // Parse startDate and endDate outside the query
            if (!DateTime.TryParse(startDate, out parsedStartDate) ||
                !DateTime.TryParse(endDate, out parsedEndDate))
            {
                BadRequest("Invalid date format.");
            }

            // Query policies with filtered criteria
            var policies = await _context.Policies
                .Where(x => staffIds.Contains(x.StaffId) &&
                            x.ExpirationDate >= parsedStartDate &&
                            x.ExpirationDate <= parsedEndDate)
                .OrderBy(x => x.PolicyNo)
                .Select(x => new
                {
                    x.PolicyNo,
                    x.PolicyType,
                    x.PolicyTitle,
                    x.ExpirationDate,
                    x.AnualPremium
                })
                .ToListAsync();

            if (policies.Any())
            {
                Table table = new Table();

                // Define table properties
                TableProperties tableProperties = new TableProperties(
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.Nil },
                        new BottomBorder() { Val = BorderValues.Nil },
                        new LeftBorder() { Val = BorderValues.Nil },
                        new RightBorder() { Val = BorderValues.Nil },
                        new InsideHorizontalBorder() { Val = BorderValues.Nil },
                        new InsideVerticalBorder() { Val = BorderValues.Nil }
                    )
                );
                table.AppendChild(tableProperties);

                // Create rows for each policy
                foreach (var policy in policies)
                {
                    TableRow policyRow = new TableRow();
                    policyRow.Append(
                        CreateTableCell(policy.PolicyNo?.ToString() ?? "N/A", false, "3100"),
                        CreateTableCell(policy.PolicyType.ToString() ?? "N/A", false, "5000"),
                        CreateTableCell(policy.PolicyTitle ?? "N/A", false, "5000"),
                        CreateTableCell(policy.ExpirationDate?.ToString("MM/dd/yyyy") ?? "N/A", false, "3100"),
                        CreateTableCell($"${policy.AnualPremium?.ToString("N2") ?? "0.00"}", false, "3100")
                    );
                    table.Append(policyRow);
                }

                body.AppendChild(table);
            }
            else
            {
                // If no policies found, add a message
                Paragraph noPolicies = new Paragraph(
                    new Run(new Text("No policies available for this Carrier."))
                );
                body.AppendChild(noPolicies);
            }

            // Add a page break after the section
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }

        private async Task AddExpirationHeader(MainDocumentPart mainPart, int[] staffIds, string startDate, string endDate)
        {
            var broker = _context.Staff.Where(x => staffIds.Contains(x.StaffId))
                            .Select(x => x.Name);
            var brokerNames = string.Join(", ", _context.Staff
                                .Where(x => staffIds.Contains(x.StaffId))
                                .Select(x => x.Name));

            HeaderPart headerPart = mainPart.AddNewPart<HeaderPart>();
            string headerPartId = mainPart.GetIdOfPart(headerPart);

            // Create the header
            Header header = new Header();

            Paragraph headerParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Arial Black" },
                        new FontSize() { Val = "36" }, // Large font size
                        new Bold()
                    ),
                    new Text("POLICIES BY EXPIRATION LISTING")
                )
            );

            header.AppendChild(headerParagraph);

            // Create a table with two columns (Broker Name on left, Date on right)
            Table headingTable = new Table();

            // Define table properties (no borders for cleaner look)
            TableProperties tblProperties = new TableProperties(
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Auto }
            );
            headingTable.AppendChild(tblProperties);

            // Create the row
            TableRow row = new TableRow();

            // Create Broker Name cell (Left-aligned)
            TableCell brokerCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "70%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Left }
                    ),
                    new Run(
                        new RunProperties(
                            //new RunFonts() { Ascii = "Arial Black" },
                            new FontSize() { Val = "28" } // 40px font size
                        ),
                        new Text($"Consultant : {brokerNames}")
                    )
                )
            );

            // Create Date Cell (Right-aligned)
            TableCell dateCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "30%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Right }
                    ),
                    new Run(
                        new RunProperties(
                            new FontSize() { Val = "28" } // Slightly smaller font
                        ),
                        new Text($"Start Date : {startDate} End Date : {endDate}")
                    )
                )
            );

            // Append cells to row
            row.Append(brokerCell, dateCell);

            // Append row to table
            headingTable.Append(row);

            // Append table to header
            header.AppendChild(headingTable);

            // Add a horizontal line
            Paragraph horizontalLine = new Paragraph(
                new ParagraphProperties(
                    new ParagraphBorders(
                        new BottomBorder() { Val = BorderValues.Single, Color = "000000", Size = 12 } // Black single-line border
                    )
                )
            );

            header.AppendChild(horizontalLine);

            //For table column 
            Table table = new Table();

            // Define table properties
            TableProperties tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Nil },
                    new BottomBorder() { Val = BorderValues.Nil },
                    new LeftBorder() { Val = BorderValues.Nil },
                    new RightBorder() { Val = BorderValues.Nil },
                    new InsideHorizontalBorder() { Val = BorderValues.Nil },
                    new InsideVerticalBorder() { Val = BorderValues.Nil }
                )
            );
            table.AppendChild(tableProperties);

            // Create the header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateTableCell("Policy Number", true, "5000"),
                CreateTableCell("Type of Insurance", true, "4000"),
                CreateTableCell("Policy Title", true, "5000"),
                CreateTableCell("Expiration", true, "3100"),
                CreateTableCell("Annual Premium", true, "3100")
            );
            table.Append(headerRow);
            header.AppendChild(table);

            // Set header content
            headerPart.Header = header;
            headerPart.Header.Save();

            // Apply the header to all pages
            SectionProperties sectionProperties = mainPart.Document.Body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
            HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = headerPartId };

            //sectionProperties.PrependChild(headerReference);
            mainPart.Document.Body.AppendChild(sectionProperties);
        }

        private void AddStaffReport(Body body, int staffId, string startDate, string endDate)
        {
            // Page Break for new staff
            if (body.Elements<Paragraph>().Any())
            {
                body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
            }
            var staffName = string.Join(", ", _context.Staff
                           .Where(x => x.StaffId == staffId)
                           .Select(x => x.Name));

            // Add Title
            Paragraph title = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(new RunProperties(new Bold(), new FontSize() { Val = "26" }), new Text("POLICY EXPIRATION LISTING"))
            );
            body.Append(title);

            // Add Header (Consultant Name and Date)
            Table headerTable = new Table();
            headerTable.Append(CreateTableProperties());

            TableRow headerRow = new TableRow(
                            new TableRowProperties(new TableHeader()) // Makes the header repeat on new pages
                        );

            headerRow.Append(CreateTableCell($"Staff: {staffName}", false, false,false,true));
            headerRow.Append(CreateTableCell("", false, false));
            headerRow.Append(CreateTableCell("", false, false));
            headerRow.Append(CreateTableCell("", false, false));
            headerRow.Append(CreateTableCell(DateTime.Now.ToString("MM/dd/yy"), false, false, true));
            headerTable.Append(headerRow);
            body.Append(headerTable);

            // Add Table
            Table dataTable = new Table();
            dataTable.Append(CreateTableProperties());

            // Add Column Headers with Bottom Border
            TableRow columnHeaders = new TableRow(
                new TableRowProperties(new TableHeader()),
             CreateTableCellForExpiration("Policy Number", false, true, 2, 10,"Arial",null),
             CreateTableCellForExpiration("Type of Insurance", false, true, 2, 10, "Arial", null),
             CreateTableCellForExpiration("Policy Title", false, true, 2, 10,"Arial", "4000"),
             CreateTableCellForExpiration("Expiration", false, true, 2, 10,"Arial", null),
             CreateTableCellForExpiration("Annual Premium", false, true,1,10,"Arial", null)
            );
            dataTable.Append(columnHeaders);

            // Add Sample Data (Replace with actual data)
            //List<PolicyData> samplePolicies = GetSampleData(staffId);
            DateTime parsedStartDate = DateTime.MinValue;
            DateTime parsedEndDate = DateTime.MaxValue;

            // Parse startDate and endDate outside the query
            if (!DateTime.TryParse(startDate, out parsedStartDate) ||
                !DateTime.TryParse(endDate, out parsedEndDate))
            {
                return;
            }

            //var policies = _context.Policies
            //    .Where(x => x.StaffId == staffId &&
            //                x.ExpirationDate >= parsedStartDate &&
            //                x.ExpirationDate <= parsedEndDate)
            //    .Select(x => new
            //    {
            //        x.PolicyNo,
            //        x.PolicyType,
            //        x.PolicyTitle,
            //        x.ExpirationDate,
            //        x.AnualPremium,
            //        TemplateDescription = _context.TemplatePrincipals
            //                                      .Where(td => td.PrincipalId == x.PrincipalId)
            //                                      .Select(td => td.Description)
            //                                      .FirstOrDefault() // Assuming you want only the first matching description
            //    })
            //    .ToList();

            var policiesByClient = _context.Policies
                                    .Where(x => x.StaffId == staffId &&
                                                x.ExpirationDate >= parsedStartDate &&
                                                x.ExpirationDate <= parsedEndDate &&
                                                x.Expired == false &&
                                                 _context.Clients.Any(c => c.ClientId == x.ClientId && c.Active)) 
                                    .GroupBy(x => x.ClientId) // Grouping by ClientId
                                    .Select(group => new
                                    {
                                        CompanyName = _context.Clients
                                                             .Where(c => c.ClientId == group.Key) 
                                                             .Select(c => c.CompanyName)
                                                             .FirstOrDefault() ?? "Unknown", // Using CompanyName instead of ClientId
                                        Policies = group.Select(x => new
                                        {
                                            x.PolicyNo,
                                            x.PolicyType,
                                            x.PolicyTitle,
                                            x.ExpirationDate,
                                            x.AnualPremium,
                                            TemplateDescription = _context.TemplatePrincipals
                                                                          .Where(td => td.PrincipalId == x.PrincipalId)
                                                                          .Select(td => td.Description)
                                                                          .FirstOrDefault() ?? "N/A" // Ensures no null values
                                        }).ToList()
                                    })
                                    .OrderBy(x => x.CompanyName) // Ordering by CompanyName
                                    .ToList();


            double subtotal = 0; // Initialize subtotal

            if (policiesByClient.Any())
            {
                foreach (var clientGroup in policiesByClient)
                {
                    TableRow emptyRow = new TableRow(
                        CreateTableCellForExpiration(" ", false, false, 1, 9, "Arial", null)
                    );
                    dataTable.Append(emptyRow);

                    TableRow clientNameRow = new TableRow(
                    CreateTableCellForExpiration(clientGroup.CompanyName ?? "", true, false,2,9, "Arial","5000"),
                    CreateTableCellForExpiration("", false, false),
                    CreateTableCellForExpiration("", false, false),
                    CreateTableCellForExpiration("", false, false)
                    );
                    dataTable.Append(clientNameRow);

                    foreach (var policy in clientGroup.Policies)
                    {
                        subtotal += policy.AnualPremium ?? 0;

                        
                        TableRow row = new TableRow(
                            CreateTableCellForExpiration(policy.PolicyNo?.ToString() ?? "N/A", false, false, 2, 9, "Arial",null,true),
                            CreateTableCellForExpiration(policy?.TemplateDescription?.ToString() ?? "N/A", false, false, 2, 9,"Arial", null, true),
                            CreateTableCellForExpiration(policy.PolicyTitle ?? "N/A", false, false, 2, 9, "Arial", "4000", true),
                            CreateTableCellForExpiration(policy.ExpirationDate?.ToString("MM/dd/yyyy") ?? "N/A", false, false, 2, 9,"Arial", null, true),
                            CreateTableCellForExpiration($"${policy.AnualPremium?.ToString("N0") ?? "0"}", false, false, 1, 9,"Arial", null, true)
                        );
                        dataTable.Append(row);
                    }
                   
                }
            }

            // Subtotal row
            TableRow rowSubtotal = new TableRow(
                CreateTableCell("", false, false),
                CreateTableCell("", false, false),
                CreateTableCell("", false, false),
                CreateTableCell("Subtotal:", true, false, false),
                CreateTableCell($"${subtotal:N0}", true, false, true)
            );
            dataTable.Append(rowSubtotal);

            body.Append(dataTable);


        }

        private static TableProperties CreateTableProperties()
        {
            return new TableProperties(
                //new TableBorders(
                //    new TopBorder() { Val = BorderValues.Single, Size = 8 },
                //    new BottomBorder() { Val = BorderValues.Single, Size = 8 },
                //    new LeftBorder() { Val = BorderValues.Single, Size = 8 },
                //    new RightBorder() { Val = BorderValues.Single, Size = 8 },
                //    new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 4 },
                //    new InsideVerticalBorder() { Val = BorderValues.Single, Size = 4 }
                //),
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct }
            );
        }

        private static TableCell CreateTableCellForExpiration(string text, bool isBold = false, bool hasBottomBorder = false,int aligned = 0, int? fontSize = 10, string? font = "", string? width = "3000", bool isBoxed = false)
        {
            RunProperties runProperties = new RunProperties();
            if (isBold) runProperties.Append(new Bold());

            // Set font if provided
            if (!string.IsNullOrEmpty(font))
            {
                runProperties.Append(new RunFonts() { Ascii = font, HighAnsi = font });
            }

            // Set font size (OpenXML uses half-point sizes)
            if (fontSize.HasValue)
            {
                runProperties.Append(new FontSize() { Val = (fontSize.Value * 2).ToString() });
            }

            ParagraphProperties paragraphProperties = new ParagraphProperties();
            // Set text alignment
            switch (aligned)
            {
                case 1:
                    paragraphProperties.Append(new Justification() { Val = JustificationValues.Right });
                    break;
                case 2:
                    paragraphProperties.Append(new Justification() { Val = JustificationValues.Left });
                    break;
                default:
                    paragraphProperties.Append(new Justification() { Val = JustificationValues.Center });
                    break;
            }

            // Reduce space between lines
            paragraphProperties.Append(new SpacingBetweenLines()
            {
                Before = "0", // No extra space before
                After = "0",  // No extra space after
                Line = "237", // Reduce line spacing (lower value makes it more compact)
                LineRule = LineSpacingRuleValues.Auto
            });

            // Ensure text wrapping for long text
            TableCellProperties cellProperties = new TableCellProperties(
                new TableCellWidth() { Width = width, Type = TableWidthUnitValues.Dxa }, // Adjust width as needed
                new Shading() { Fill = "FFFFFF" } // Optional: Background color
            );

            if (hasBottomBorder)
            {
                cellProperties.Append(new TableCellBorders(
                    new TopBorder() { Val = BorderValues.Single, Size = 8 }
                ));

                //runProperties.Append(new Underline() { Val = UnderlineValues.Single }); // Add underline to text

                cellProperties.Append(new TableCellBorders(
                   new BottomBorder() { Val = BorderValues.Single, Size = 8 }
               ));
            }

            // Add full box-style borders if isBoxed is true
            if (isBoxed)
            {
                cellProperties.Append(new TableCellBorders(
                    new TopBorder() { Val = BorderValues.Single, Size = 8 },
                    new BottomBorder() { Val = BorderValues.Single, Size = 8 },
                    new LeftBorder() { Val = BorderValues.Single, Size = 8 },
                    new RightBorder() { Val = BorderValues.Single, Size = 8 }
                ));

                cellProperties.Append(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
            }

            return new TableCell(
                cellProperties,
                new Paragraph(paragraphProperties,
                    new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve }) // Preserve spaces for better wrapping
                )
            );
        }

        private static TableCell CreateTableCell(string text, bool isBold = false, bool hasBottomBorder = false, bool isRightAligned = false,bool? isLeftAligned = false)
        {
            RunProperties runProperties = new RunProperties();
            if (isBold) runProperties.Append(new Bold());

            ParagraphProperties paragraphProperties = new ParagraphProperties();
            if (isRightAligned)
            {
                paragraphProperties.Append(new Justification() { Val = JustificationValues.Right });
            }
            else if(isLeftAligned == true)
            {
                paragraphProperties.Append(new Justification() { Val = JustificationValues.Left });
            }
            else
            {
                paragraphProperties.Append(new Justification() { Val = JustificationValues.Center });
            }

            // Ensure text wrapping for long text
            TableCellProperties cellProperties = new TableCellProperties(
                new TableCellWidth() { Width = "3000", Type = TableWidthUnitValues.Dxa }, // Adjust width as needed
                new SpacingBetweenLines() { Before = "0", After = "0", Line = "220", LineRule = LineSpacingRuleValues.Auto }, // Reduce space between lines
                new Shading() { Fill = "FFFFFF" } // Optional: Background color
            );

            if (hasBottomBorder)
            {
                cellProperties.Append(new TableCellBorders(
                    new TopBorder() { Val = BorderValues.Single, Size = 8 }
                ));

                runProperties.Append(new Underline() { Val = UnderlineValues.Single }); // Add underline to text
            }

            return new TableCell(
                cellProperties,
                new Paragraph(paragraphProperties,
                    new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve }) // Preserve spaces for better wrapping
                )
            );
        }

        private static List<PolicyData> GetSampleData(int staffId)
        {
            return new List<PolicyData>
        {
            new PolicyData { PolicyNumber = "S2334662", TypeOfInsurance = "COMMERCIAL GENERAL LIABILITY", PolicyTitle = "10 MILL STREET", ExpirationDate = "05/31/25", AnnualPremium = "$9,256" },
            new PolicyData { PolicyNumber = "CIUBEA0014094", TypeOfInsurance = "HOMEOWNERS", PolicyTitle = "6799 COLLINS AVE", ExpirationDate = "05/30/25", AnnualPremium = "$630" },
            new PolicyData { PolicyNumber = "1000305792241", TypeOfInsurance = "COMMERCIAL GENERAL LIABILITY", PolicyTitle = "MIDDLETOWN HOTEL", ExpirationDate = "05/01/25", AnnualPremium = "$194,740" },
            new PolicyData { PolicyNumber = "VARIOUS", TypeOfInsurance = "COMMERCIAL UMBRELLA", PolicyTitle = "MIDDLETOWN HOTEL", ExpirationDate = "05/01/25", AnnualPremium = "$250,929" }
        };
        }
        #endregion

        [HttpGet("GenerateClientReport")]
        public async Task<IActionResult> GenerateClientReport(string startDate, string endDate)
        {
            try
            {
                var convStartDate = DateTime.Parse(startDate);
                var convEndDate = DateTime.Parse(endDate);
                byte[] fileBytes = await GenerateClientWordDocumentAsync(convStartDate, convEndDate);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Client_Report.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GenerateClientWordDocumentAsync(DateTime startDate, DateTime endDate)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // Add the header to the document
                    await AddClientHeader(mainPart);

                    await AddClientSection(body, startDate, endDate);

                    // Ensure TOC updates when opening the document
                    AddUpdateFieldsOnOpen(mainPart);

                    //AddFooterToAllPages(mainPart);

                    mainPart.Document.Save();
                }

                return memoryStream.ToArray();
            }
        }

        private async Task AddClientSection(Body body, DateTime startDate, DateTime endDate)
        {
            // Fetch clients associated with the broker
            var clients = await _context.Clients
                .Where(x => x.AccountOpenDate >= startDate && x.AccountOpenDate <= endDate)
                .Select(x => new { x.ClientAcctId, x.CompanyName, x.Fax, x.AccountOpenDate, x.Comments })
                .OrderBy(x=>x.AccountOpenDate)
                .ToListAsync();

            if (clients.Any())
            {
                Table table = new Table();

                // Define table properties
                TableProperties tableProperties = new TableProperties(
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.Nil },
                        new BottomBorder() { Val = BorderValues.Nil },
                        new LeftBorder() { Val = BorderValues.Nil },
                        new RightBorder() { Val = BorderValues.Nil },
                        new InsideHorizontalBorder() { Val = BorderValues.Nil },
                        new InsideVerticalBorder() { Val = BorderValues.Nil }
                    )
                );
                table.AppendChild(tableProperties);

                // Create rows for each policy
                foreach (var client in clients)
                {
                    TableRow policyRow = new TableRow();
                    policyRow.Append(
                        CreateCarrierBrokerTableCell(client.ClientAcctId.ToString() ?? "N/A", false, "2000",false,false),
                        CreateCarrierBrokerTableCell(client.CompanyName.ToString() ?? "N/A", false, "4000", false, false),
                        CreateCarrierBrokerTableCell(client.Fax ?? "N/A", false, "4000", false, false),
                        CreateCarrierBrokerTableCell(Convert.ToDateTime(client.AccountOpenDate).ToString("MM/dd/yyyy") ?? "N/A", false, "3100", false, false),
                        CreateCarrierBrokerTableCell($"{client.Comments?.ToString() ?? " "}", false, "4500", false, false)
                    );
                    table.Append(policyRow);
                }

                body.AppendChild(table);
            }
            else
            {
                // If no policies found, add a message
                Paragraph noPolicies = new Paragraph(
                    new Run(new Text("No Clients available."))
                );
                body.AppendChild(noPolicies);
            }

            // Add a page break after the section
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }

        private async Task AddClientHeader(MainDocumentPart mainPart)
        {
            HeaderPart headerPart = mainPart.AddNewPart<HeaderPart>();
            string headerPartId = mainPart.GetIdOfPart(headerPart);

            // Create the header
            Header header = new Header();

            Paragraph headerParagraph = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Arial Black" },
                        new FontSize() { Val = "36" }, // Large font size
                        new Bold()
                    ),
                    new Text("CLIENT LISTING")
                )
            );

            header.AppendChild(headerParagraph);

            // Create a table with two columns (Broker Name on left, Date on right)
            Table headingTable = new Table();

            // Define table properties (no borders for cleaner look)
            TableProperties tblProperties = new TableProperties(
                new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Auto }
            );
            headingTable.AppendChild(tblProperties);

            // Create the row
            TableRow row = new TableRow();

            // Create Broker Name cell (Left-aligned)
            TableCell brokerCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "70%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Left }
                    ),
                    new Run(
                        new RunProperties(
                            //new RunFonts() { Ascii = "Arial Black" },
                            new FontSize() { Val = "28" } // 40px font size
                        ),
                        new Text($" ")
                    )
                )
            );

            // Create Date Cell (Right-aligned)
            TableCell dateCell = new TableCell(
                new TableCellProperties(new TableCellWidth() { Width = "30%", Type = TableWidthUnitValues.Dxa }),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Right }
                    ),
                    new Run(
                        new RunProperties(
                            new FontSize() { Val = "28" } // Slightly smaller font
                        ),
                        new Text(DateTime.Now.ToString("MM/dd/yy"))
                    )
                )
            );

            // Append cells to row
            row.Append(brokerCell, dateCell);

            // Append row to table
            headingTable.Append(row);

            // Append table to header
            header.AppendChild(headingTable);

            // Add a horizontal line
            Paragraph horizontalLine = new Paragraph(
                new ParagraphProperties(
                    new ParagraphBorders(
                        new BottomBorder() { Val = BorderValues.Single, Color = "000000", Size = 12 } // Black single-line border
                    )
                )
            );

            header.AppendChild(horizontalLine);

            //For table column 
            Table table = new Table();

            // Define table properties
            TableProperties tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Nil },
                    new BottomBorder() { Val = BorderValues.Nil },
                    new LeftBorder() { Val = BorderValues.Nil },
                    new RightBorder() { Val = BorderValues.Nil },
                    new InsideHorizontalBorder() { Val = BorderValues.Nil },
                    new InsideVerticalBorder() { Val = BorderValues.Nil }
                )
            );
            table.AppendChild(tableProperties);

            // Create the header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateCarrierBrokerTableCell("Client ID", false, "2000",false,true),
                CreateCarrierBrokerTableCell("Company Name", false, "4000",false, true),
                CreateCarrierBrokerTableCell("Phone/Fax #", false, "4000", false, true),
                CreateCarrierBrokerTableCell("Aquired Date", false, "3100", false, true),
                CreateCarrierBrokerTableCell("Contact/Accts Recv", false, "4500", false, true)
            );
            table.Append(headerRow);
            header.AppendChild(table);

            // Set header content
            headerPart.Header = header;
            headerPart.Header.Save();

            // Apply the header to all pages
            SectionProperties sectionProperties = mainPart.Document.Body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
            HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = headerPartId };

            sectionProperties.PrependChild(headerReference);
            mainPart.Document.Body.AppendChild(sectionProperties);
        }

    }

    public class PolicyData
    {
        public string PolicyNumber { get; set; }
        public string TypeOfInsurance { get; set; }
        public string PolicyTitle { get; set; }
        public string ExpirationDate { get; set; }
        public string AnnualPremium { get; set; }
    }
}
