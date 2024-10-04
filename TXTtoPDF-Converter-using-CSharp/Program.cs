using System;
using System.IO;
using System.Configuration;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace TXTtoPDF_Converter_using_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read source and destination paths from app.config
            string sourcePath = ConfigurationManager.AppSettings["SourcePath"];
            string destinationPath = ConfigurationManager.AppSettings["DestinationPath"];

            // Check if destination folder exists, create if not
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // Get all .txt files in source folder
            string[] txtFiles = Directory.GetFiles(sourcePath, "*.txt");

            if (txtFiles.Length == 0)
            {
                Console.WriteLine("No TXT files found in the source folder.");
                return;
            }

            foreach (var txtFilePath in txtFiles)
            {
                try
                {
                    // Get file name without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(txtFilePath);
                    string pdfFilePath = Path.Combine(destinationPath, fileNameWithoutExtension + ".pdf");

                    // Check if the PDF already exists
                    if (File.Exists(pdfFilePath))
                    {
                        Console.WriteLine($"PDF already exists for {fileNameWithoutExtension}. Skipping file.");
                        continue;
                    }

                    // Convert TXT file to PDF
                    ConvertTxtToPdf(txtFilePath, pdfFilePath);

                    // After conversion, delete the source TXT file
                    File.Delete(txtFilePath);

                    Console.WriteLine($"Successfully converted {fileNameWithoutExtension}.txt to PDF.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting {txtFilePath}: {ex.Message}");
                }
            }
        }

        // Convert TXT file to PDF using PdfSharp
        static void ConvertTxtToPdf(string txtFilePath, string pdfFilePath)
        {
            // Read text content from the TXT file
            string txtContent = File.ReadAllText(txtFilePath);

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = Path.GetFileNameWithoutExtension(txtFilePath);

            // Create an empty page
            PdfPage page = document.AddPage();

            // Create a graphics object for drawing text
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Verdana", 12);

            // Define layout variables
            double margin = 40;  // Margin from left and right
            double yPos = 40;    // Starting position for text (from top)
            double pageWidth = page.Width - margin * 2;

            // Split the content into lines, considering word wrapping
            string[] lines = WrapText(txtContent, font, gfx, pageWidth);

            foreach (var line in lines)
            {
                if (yPos + font.Height > page.Height - margin) // Add new page if needed
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = margin; // Reset y position for new page
                }

                // Draw the text content line by line
                gfx.DrawString(line.TrimEnd(), font, XBrushes.Black,
                    new XRect(margin, yPos, pageWidth, page.Height),
                    XStringFormats.TopLeft);

                yPos += font.Height;
            }

            // Save the PDF file
            document.Save(pdfFilePath);
        }

        // Helper function to wrap text based on page width
        static string[] WrapText(string text, XFont font, XGraphics gfx, double maxWidth)
        {
            // Replace multiple spaces with a single space
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // Correct usage
            var lines = new System.Collections.Generic.List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var size = gfx.MeasureString(testLine, font);

                if (size.Width > maxWidth)
                {
                    if (!string.IsNullOrWhiteSpace(currentLine)) // Check if currentLine is not empty
                    {
                        lines.Add(currentLine.TrimEnd()); // Trim trailing spaces before adding
                    }
                    currentLine = word; // Start a new line with the current word
                }
                else
                {
                    currentLine = testLine;
                }
            }

            // Add the last line
            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine.TrimEnd()); // Trim trailing spaces
            }

            return lines.ToArray();
        }
    }
}