namespace PdfProcessing.Shared.Options;

public sealed class PdfStorageOptions
{
    public const string SectionName = "PdfStorage";

    public string UploadDirectory { get; set; } = "/data/uploads";
}
