using Microsoft.Extensions.Options;
using PdfProcessing.Shared.Options;

namespace PdfProcessing.ApiGateway.Storage;

public sealed class PdfFileStorage(IOptions<PdfStorageOptions> options) : IPdfFileStorage
{
    public async Task<StoredPdfFile> SaveAsync(
        IFormFile file,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var uploadDirectory = options.Value.UploadDirectory;
        Directory.CreateDirectory(uploadDirectory);

        var originalFileName = Path.GetFileName(file.FileName);
        var storedFileName = $"{documentId}.pdf";
        var filePath = Path.Combine(uploadDirectory, storedFileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream, cancellationToken);

        return new StoredPdfFile(originalFileName, storedFileName, filePath);
    }
}
