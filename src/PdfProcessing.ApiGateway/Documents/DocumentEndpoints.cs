using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PdfProcessing.ApiGateway.Messaging;
using PdfProcessing.ApiGateway.Storage;
using PdfProcessing.Shared.Documents;
using PdfProcessing.Shared.Messaging;

namespace PdfProcessing.ApiGateway.Documents;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/documents")
            .WithTags("Documents");

        group.MapPost("", UploadDocumentAsync)
            .WithName("UploadDocument")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<DocumentSummaryDto>(StatusCodes.Status202Accepted)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .WithOpenApi();

        group.MapGet("", ListDocumentsAsync)
            .WithName("ListDocuments")
            .Produces<IReadOnlyList<DocumentSummaryDto>>()
            .WithOpenApi();

        group.MapGet("/{id:guid}/text", GetDocumentTextAsync)
            .WithName("GetDocumentText")
            .Produces<DocumentTextDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces<DocumentProcessingStateDto>(StatusCodes.Status409Conflict)
            .WithOpenApi();
    }

    private static async Task<Results<Accepted<DocumentSummaryDto>, BadRequest<string>>> UploadDocumentAsync(
        IFormFile? file,
        DocumentDbContext db,
        IPdfFileStorage storage,
        IPdfProcessingPublisher publisher,
        CancellationToken cancellationToken)
    {
        var validation = PdfUploadValidator.Validate(file);
        if (!validation.IsValid)
        {
            return TypedResults.BadRequest(validation.ErrorMessage);
        }

        var documentId = Guid.NewGuid();
        var storedFile = await storage.SaveAsync(file!, documentId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = storedFile.OriginalFileName,
            StoredFileName = storedFile.StoredFileName,
            FilePath = storedFile.FilePath,
            Status = DocumentStatus.Uploaded,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Documents.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new PdfProcessingMessage(document.Id, document.FilePath),
            cancellationToken);

        return TypedResults.Accepted($"/api/documents/{document.Id}/text", DocumentDtos.ToSummaryDto(document));
    }

    private static async Task<Ok<IReadOnlyList<DocumentSummaryDto>>> ListDocumentsAsync(
        DocumentDbContext db,
        CancellationToken cancellationToken)
    {
        
        var documents = await db.Documents
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => DocumentDtos.ToSummaryDto(x))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyList<DocumentSummaryDto>>(documents);
    }

    private static async Task<Results<Ok<DocumentTextDto>, NotFound, Conflict<DocumentProcessingStateDto>>> GetDocumentTextAsync(
        Guid id,
        DocumentDbContext db,
        CancellationToken cancellationToken)
    {
        var document = await db.Documents.FindAsync([id], cancellationToken);
        if (document is null)
        {
            return TypedResults.NotFound();
        }

        if (document.Status != DocumentStatus.Completed)
        {
            return TypedResults.Conflict(new DocumentProcessingStateDto(
                document.Id,
                document.Status.ToString(),
                document.ErrorMessage));
        }

        return TypedResults.Ok(DocumentDtos.ToTextDto(document));
    }
}
