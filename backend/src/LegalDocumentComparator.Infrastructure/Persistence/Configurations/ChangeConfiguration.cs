using LegalDocumentComparator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegalDocumentComparator.Infrastructure.Persistence.Configurations;

public class ChangeConfiguration : IEntityTypeConfiguration<Change>
{
    public void Configure(EntityTypeBuilder<Change> builder)
    {
        builder.ToTable("Changes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Severity)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.PageNumber)
            .IsRequired();

        builder.Property(c => c.OldText)
            .HasMaxLength(5000);

        builder.Property(c => c.NewText)
            .HasMaxLength(5000);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.LegalMeaning)
            .HasMaxLength(5000);

        builder.Property(c => c.PositionX).IsRequired();
        builder.Property(c => c.PositionY).IsRequired();
        builder.Property(c => c.Width).IsRequired();
        builder.Property(c => c.Height).IsRequired();
    }
}
