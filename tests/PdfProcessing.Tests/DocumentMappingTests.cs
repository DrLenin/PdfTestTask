using PdfProcessing.Shared.Documents;

namespace PdfProcessing.Tests;

public sealed class DocumentMappingTests
{
    [Fact]
    public void ToSummaryDto_MapsDocumentFields()
    {
        var now = new DateTimeOffset(2026, 5, 7, 10, 0, 0, TimeSpan.Zero);
        var document = new Document
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            OriginalFileName = "sample.pdf",
            StoredFileName = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb.pdf",
            FilePath = "/data/uploads/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb.pdf",
            Status = DocumentStatus.Completed,
            TextContent = "Extracted text",
            ErrorMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var dto = DocumentDtos.ToSummaryDto(document);

        Assert.Equal(document.Id, dto.Id);
        Assert.Equal("sample.pdf", dto.OriginalFileName);
        Assert.Equal("Completed", dto.Status);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Equal(now, dto.UpdatedAt);
        Assert.Null(dto.ErrorMessage);
    }
}
