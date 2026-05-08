namespace PdfProcessing.Shared.Messaging;

public sealed record PdfProcessingMessage(Guid DocumentId, string FilePath);
