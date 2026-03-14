namespace LegalDocumentComparator.Domain.Entities;

public class AnalysisResult
{
    public Guid Id { get; private set; }
    public Guid ComparisonId { get; private set; }
    public string Summary { get; private set; } = null!;
    public string LegalImplications { get; private set; } = null!;
    public string RiskAssessment { get; private set; } = null!;
    public string KeyChangesJson { get; private set; } = null!;
    public DateTime GeneratedAt { get; private set; }

    public Comparison Comparison { get; private set; } = null!;

    private AnalysisResult() { }

    public static AnalysisResult Create(
        Guid comparisonId,
        string summary,
        string legalImplications,
        string riskAssessment,
        string keyChangesJson)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty", nameof(summary));

        return new AnalysisResult
        {
            Id = Guid.NewGuid(),
            ComparisonId = comparisonId,
            Summary = summary,
            LegalImplications = legalImplications ?? string.Empty,
            RiskAssessment = riskAssessment ?? string.Empty,
            KeyChangesJson = keyChangesJson ?? "[]",
            GeneratedAt = DateTime.UtcNow
        };
    }
}
