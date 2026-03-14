using LegalDocumentComparator.Domain.Enums;
using LegalDocumentComparator.Domain.ValueObjects;

namespace LegalDocumentComparator.Application.Common.Models;

public class TextDifference
{
    public ChangeType Type { get; set; }
    public int PageNumber { get; set; }
    public string OldText { get; set; } = string.Empty;
    public string NewText { get; set; } = string.Empty;
    public TextPosition Position { get; set; } = null!;
}
