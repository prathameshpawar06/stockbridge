using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Pictures;

namespace stockbridge_api.Services
{
    public class AddLogoService
    {
        // Adding the logo image to the Word document
        public DocumentFormat.OpenXml.Wordprocessing.Paragraph AddImageToDocument(MainDocumentPart mainPart, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file not found at path: {imagePath}");
            }

            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            // Get the relationship ID of the image
            string relationshipId = mainPart.GetIdOfPart(imagePart);

            // Get original image dimensions
            using (System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath))
            {
                const long emuPerInch = 914400L; // EMU units per inch
                const float dpi = 96f; // Typical DPI for images

                long originalWidth = (long)(img.Width * emuPerInch / dpi);
                long originalHeight = (long)(img.Height * emuPerInch / dpi);

                // Set a maximum width (e.g., 5 inches)
                long maxWidth = 5 * emuPerInch;

                long cx, cy;
                if (originalWidth > maxWidth)
                {
                    // Scale height proportionally
                    double scaleFactor = (double)maxWidth / originalWidth;
                    cx = maxWidth;
                    cy = (long)(originalHeight * scaleFactor);
                }
                else
                {
                    cx = originalWidth;
                    cy = originalHeight;
                }

                // Decrease final size by 30%
                double reductionFactor = 0.5; // 70% of the original size

                cx = (long)(cx * reductionFactor);
                cy = (long)(cy * reductionFactor);

                // Create a new Drawing element for the image
                Drawing drawing = new Drawing(
                    new Inline(
                        new Extent() { Cx = cx, Cy = cy }, // Adjusted size
                        new EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                        new DocProperties() { Id = 1U, Name = "Picture" },
                        new Graphic(
                            new GraphicData(
                                new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties() { Id = 0U, Name = "Image.jpg" },
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()
                                    ),
                                    new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                        new DocumentFormat.OpenXml.Drawing.Blip() { Embed = relationshipId },
                                        new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())
                                    ),
                                    new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                        new DocumentFormat.OpenXml.Drawing.Transform2D(
                                            new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                            new DocumentFormat.OpenXml.Drawing.Extents() { Cx = cx, Cy = cy } // Corrected size
                                        ),
                                        new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                            new DocumentFormat.OpenXml.Drawing.AdjustValueList()
                                        )
                                        { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }
                                    )
                                )
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                        )
                    )
                    { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U }
                );

                DocumentFormat.OpenXml.Wordprocessing.Paragraph imageParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(drawing));
                return imageParagraph;
            }
        }


    }
}
