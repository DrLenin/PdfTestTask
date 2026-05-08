using PdfProcessing.Shared.Messaging;

namespace PdfProcessing.ApiGateway.Messaging;

public interface IPdfProcessingPublisher
{
    Task PublishAsync(PdfProcessingMessage message, CancellationToken cancellationToken);
}
