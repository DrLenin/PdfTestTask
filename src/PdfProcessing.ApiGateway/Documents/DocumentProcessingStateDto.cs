namespace PdfProcessing.ApiGateway.Documents;

public sealed record DocumentProcessingStateDto(Guid Id, string Status, string? ErrorMessage);
