using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PdfProcessing.Shared.Documents;
using PdfProcessing.Shared.Messaging;
using PdfProcessing.Shared.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PdfProcessing.BackgroundWorker;

public sealed class PdfProcessingWorker(
    ILogger<PdfProcessingWorker> logger,
    IOptions<RabbitMqOptions> rabbitOptions,
    IDbContextFactory<DocumentDbContext> dbContextFactory) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = rabbitOptions.Value;
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("PDF processing worker is consuming queue {QueueName}", options.QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<PdfProcessingMessage>(json);
            if (message is null)
            {
                await _channel.BasicAckAsync(args.DeliveryTag, false);
                return;
            }

            await ProcessDocumentAsync(message);
            await _channel.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected worker error while handling RabbitMQ message.");
            await _channel.BasicNackAsync(args.DeliveryTag, false, requeue: true);
        }
    }

    private async Task ProcessDocumentAsync(PdfProcessingMessage message)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var document = await db.Documents.FirstOrDefaultAsync(x => x.Id == message.DocumentId);
        if (document is null)
        {
            logger.LogWarning("Document {DocumentId} was not found; acknowledging message.", message.DocumentId);
            return;
        }

        document.Status = DocumentStatus.Processing;
        document.ErrorMessage = null;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        try
        {
            if (!File.Exists(message.FilePath))
            {
                throw new FileNotFoundException("PDF file was not found.", message.FilePath);
            }

            document.TextContent = PdfTextExtractor.ExtractText(message.FilePath);
            document.Status = DocumentStatus.Completed;
            document.ErrorMessage = null;
            document.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or UglyToad.PdfPig.Core.PdfDocumentFormatException)
        {
            document.Status = DocumentStatus.Failed;
            document.ErrorMessage = ex.Message;
            document.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            logger.LogWarning(ex, "Document {DocumentId} failed PDF processing.", document.Id);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
