using Microsoft.AspNetCore.Http;
using PdfProcessing.ApiGateway.Documents;

namespace PdfProcessing.Tests;

public sealed class PdfUploadValidatorTests
{
    [Fact]
    public void Validate_ReturnsError_WhenFileIsMissing()
    {
        var result = PdfUploadValidator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Equal("PDF file is required.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenFileIsNotPdf()
    {
        var file = CreateFile("notes.txt", "text/plain", length: 42);

        var result = PdfUploadValidator.Validate(file);

        Assert.False(result.IsValid);
        
        Assert.Equal("Only PDF files are supported.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_AllowsPdfFile()
    {
        var file = CreateFile("sample.pdf", "application/pdf", length: 42);

        var result = PdfUploadValidator.Validate(file);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    private static IFormFile CreateFile(string fileName, string contentType, long length)
    {
        return new FormFile(Stream.Null, 0, length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
