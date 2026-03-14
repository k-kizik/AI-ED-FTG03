using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.Application.Common.Models;

public class ChangeDetail
{
    public ChangeType Type { get; set; }
    public ChangeSeverity Severity { get; set; }
    public int PageNumber { get; set; }
    public string OldText { get; set; } = string.Empty;
    public string NewText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LegalMeaning { get; set; } = string.Empty;
}
