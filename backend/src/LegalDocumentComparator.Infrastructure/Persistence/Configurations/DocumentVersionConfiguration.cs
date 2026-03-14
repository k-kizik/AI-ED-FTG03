using LegalDocumentComparator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegalDocumentComparator.Infrastructure.Persistence.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(v => v.VersionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.PageCount)
            .IsRequired();

        builder.Property(v => v.FileSizeBytes)
            .IsRequired();

        builder.Property(v => v.UploadedAt)
            .IsRequired();

        builder.HasMany(v => v.OriginalComparisons)
            .WithOne(c => c.OriginalVersion)
            .HasForeignKey(c => c.OriginalVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.NewComparisons)
            .WithOne(c => c.NewVersion)
            .HasForeignKey(c => c.NewVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
