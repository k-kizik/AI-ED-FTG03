namespace LegalDocumentComparator.Application.Common.Models;

public class ComparisonAnalysis
{
    public string Summary { get; set; } = string.Empty;
    public string LegalImplications { get; set; } = string.Empty;
    public string RiskAssessment { get; set; } = string.Empty;
    public List<KeyChange> KeyChanges { get; set; } = new();
}

public class KeyChange
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
