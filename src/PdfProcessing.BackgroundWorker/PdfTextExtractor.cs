using System.Text;
using UglyToad.PdfPig;

namespace PdfProcessing.BackgroundWorker;

public sealed class PdfTextExtractor
{
    public static string ExtractText(string filePath)
    {
        var builder = new StringBuilder();

        using var document = PdfDocument.Open(filePath);
        
        foreach (var page in document.GetPages())
        {
            builder.AppendLine(page.Text);
        }

        return builder.ToString().Trim();
    }
}
