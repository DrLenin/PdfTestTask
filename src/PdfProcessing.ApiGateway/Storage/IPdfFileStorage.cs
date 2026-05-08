namespace PdfProcessing.ApiGateway.Storage;

public interface IPdfFileStorage
{
    Task<StoredPdfFile> SaveAsync(IFormFile file, Guid documentId, CancellationToken cancellationToken);
}

public sealed record StoredPdfFile(string OriginalFileName, string StoredFileName, string FilePath);
