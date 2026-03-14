namespace LegalDocumentComparator.Application.UseCases.Comparisons.CompareDocuments;

public class CompareDocumentsCommand
{
    public Guid OriginalVersionId { get; set; }
    public Guid NewVersionId { get; set; }
    public bool ForceRegenerate { get; set; }
}

public class CompareDocumentsResult
{
    public Guid ComparisonId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string LegalImplications { get; set; } = string.Empty;
    public string RiskAssessment { get; set; } = string.Empty;
    public List<KeyChangeDto> Changes { get; set; } = new();
    public bool WasGenerated { get; set; }
}

public class KeyChangeDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
