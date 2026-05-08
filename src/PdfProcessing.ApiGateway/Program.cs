using Microsoft.EntityFrameworkCore;
using PdfProcessing.ApiGateway.Documents;
using PdfProcessing.ApiGateway.Messaging;
using PdfProcessing.ApiGateway.Storage;
using PdfProcessing.Shared.Documents;
using PdfProcessing.Shared.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PdfStorageOptions>(builder.Configuration.GetSection(PdfStorageOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<IPdfFileStorage, PdfFileStorage>();
builder.Services.AddScoped<IPdfProcessingPublisher, RabbitMqPdfProcessingPublisher>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapDocumentEndpoints();

await app.RunAsync();
