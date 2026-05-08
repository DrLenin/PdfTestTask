using Microsoft.EntityFrameworkCore;
using PdfProcessing.BackgroundWorker;
using PdfProcessing.Shared.Documents;
using PdfProcessing.Shared.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddDbContextFactory<DocumentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddSingleton<PdfTextExtractor>();
builder.Services.AddHostedService<PdfProcessingWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DocumentDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}


await host.RunAsync();
