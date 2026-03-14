using LegalDocumentComparator.Domain.Enums;
using LegalDocumentComparator.Domain.ValueObjects;

namespace LegalDocumentComparator.Domain.Entities;

public class Change
{
    public Guid Id { get; private set; }
    public Guid ComparisonId { get; private set; }
    public ChangeType Type { get; private set; }
    public ChangeSeverity Severity { get; private set; }
    public int PageNumber { get; private set; }
    public string OldText { get; private set; } = string.Empty;
    public string NewText { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string LegalMeaning { get; private set; } = string.Empty;
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }

    public Comparison Comparison { get; private set; } = null!;

    private Change() { }

    public static Change Create(
        Guid comparisonId,
        ChangeType type,
        ChangeSeverity severity,
        int pageNumber,
        string oldText,
        string newText,
        string description,
        string legalMeaning,
        TextPosition position)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be positive", nameof(pageNumber));

        return new Change
        {
            Id = Guid.NewGuid(),
            ComparisonId = comparisonId,
            Type = type,
            Severity = severity,
            PageNumber = pageNumber,
            OldText = oldText ?? string.Empty,
            NewText = newText ?? string.Empty,
            Description = description ?? string.Empty,
            LegalMeaning = legalMeaning ?? string.Empty,
            PositionX = position.X,
            PositionY = position.Y,
            Width = position.Width,
            Height = position.Height
        };
    }

    public TextPosition GetPosition()
    {
        return new TextPosition(PageNumber, PositionX, PositionY, Width, Height);
    }
}
