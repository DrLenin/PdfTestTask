namespace PdfProcessing.ApiGateway.Documents;

public static class PdfUploadValidator
{
    public static PdfUploadValidationResult Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return PdfUploadValidationResult.Invalid("PDF file is required.");
        }

        var hasPdfExtension = Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        var hasPdfContentType = file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
        
        if (!hasPdfExtension && !hasPdfContentType)
        {
            return PdfUploadValidationResult.Invalid("Only PDF files are supported.");
        }

        return PdfUploadValidationResult.Valid();
    }
}

public sealed record PdfUploadValidationResult(bool IsValid, string? ErrorMessage)
{
    public static PdfUploadValidationResult Valid()
    {
        return new PdfUploadValidationResult(true, null);
    }

    public static PdfUploadValidationResult Invalid(string errorMessage)
    {
        return new PdfUploadValidationResult(false, errorMessage);
    }
}
