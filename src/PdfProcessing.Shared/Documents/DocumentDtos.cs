namespace PdfProcessing.Shared.Documents;

public sealed record DocumentSummaryDto(
    Guid Id,
    string OriginalFileName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ErrorMessage);

public sealed record DocumentTextDto(Guid Id, string OriginalFileName, string TextContent);

public static class DocumentDtos
{
    public static DocumentSummaryDto ToSummaryDto(Document document)
    {
        return new DocumentSummaryDto(
            document.Id,
            document.OriginalFileName,
            document.Status.ToString(),
            document.CreatedAt,
            document.UpdatedAt,
            document.ErrorMessage);
    }

    public static DocumentTextDto ToTextDto(Document document)
    {
        return new DocumentTextDto(
            document.Id,
            document.OriginalFileName,
            document.TextContent ?? string.Empty);
    }
}
