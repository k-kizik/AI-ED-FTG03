using LegalDocumentComparator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegalDocumentComparator.Infrastructure.Persistence.Configurations;

public class ComparisonConfiguration : IEntityTypeConfiguration<Comparison>
{
    public void Configure(EntityTypeBuilder<Comparison> builder)
    {
        builder.ToTable("Comparisons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasMany(c => c.Changes)
            .WithOne(ch => ch.Comparison)
            .HasForeignKey(ch => ch.ComparisonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AnalysisResult)
            .WithOne(a => a.Comparison)
            .HasForeignKey<AnalysisResult>(a => a.ComparisonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
