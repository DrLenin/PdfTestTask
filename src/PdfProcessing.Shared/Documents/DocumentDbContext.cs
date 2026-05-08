using Microsoft.EntityFrameworkCore;

namespace PdfProcessing.Shared.Documents;

public sealed class DocumentDbContext(DbContextOptions<DocumentDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var document = modelBuilder.Entity<Document>();
        document.ToTable("documents");
        document.HasKey(x => x.Id);
        document.Property(x => x.Id).HasColumnName("id");
        document.Property(x => x.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(255).IsRequired();
        document.Property(x => x.StoredFileName).HasColumnName("stored_file_name").HasMaxLength(255).IsRequired();
        document.Property(x => x.FilePath).HasColumnName("file_path").HasMaxLength(1024).IsRequired();
        document.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        document.Property(x => x.TextContent).HasColumnName("text_content");
        document.Property(x => x.ErrorMessage).HasColumnName("error_message");
        document.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        document.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        document.HasIndex(x => x.CreatedAt);
    }
}
