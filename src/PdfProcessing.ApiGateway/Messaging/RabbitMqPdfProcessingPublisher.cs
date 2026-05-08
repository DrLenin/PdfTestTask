using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PdfProcessing.Shared.Messaging;
using PdfProcessing.Shared.Options;
using RabbitMQ.Client;

namespace PdfProcessing.ApiGateway.Messaging;

public sealed class RabbitMqPdfProcessingPublisher(IOptions<RabbitMqOptions> options) : IPdfProcessingPublisher
{
    public async Task PublishAsync(PdfProcessingMessage message, CancellationToken cancellationToken)
    {
        var rabbitOptions = options.Value;
        
        var factory = new ConnectionFactory
        {
            HostName = rabbitOptions.HostName,
            Port = rabbitOptions.Port,
            UserName = rabbitOptions.UserName,
            Password = rabbitOptions.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: rabbitOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: rabbitOptions.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
