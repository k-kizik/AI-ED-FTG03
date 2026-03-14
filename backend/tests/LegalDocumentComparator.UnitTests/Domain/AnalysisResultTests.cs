using FluentAssertions;
using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.UnitTests.Domain;

public class AnalysisResultTests
{
    private readonly Guid _comparisonId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_ReturnsAnalysisResultWithCorrectProperties()
    {
        var result = AnalysisResult.Create(_comparisonId, "Summary text", "Legal implications", "High risk", "[{}]");

        result.Id.Should().NotBeEmpty();
        result.ComparisonId.Should().Be(_comparisonId);
        result.Summary.Should().Be("Summary text");
        result.LegalImplications.Should().Be("Legal implications");
        result.RiskAssessment.Should().Be("High risk");
        result.KeyChangesJson.Should().Be("[{}]");
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSummary_ThrowsArgumentException(string? summary)
    {
        var act = () => AnalysisResult.Create(_comparisonId, summary!, "implications", "risk", "[]");

        act.Should().Throw<ArgumentException>().WithParameterName("summary");
    }

    [Fact]
    public void Create_WithNullLegalImplications_DefaultsToEmpty()
    {
        var result = AnalysisResult.Create(_comparisonId, "Summary", null!, "risk", "[]");

        result.LegalImplications.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullRiskAssessment_DefaultsToEmpty()
    {
        var result = AnalysisResult.Create(_comparisonId, "Summary", "implications", null!, "[]");

        result.RiskAssessment.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullKeyChangesJson_DefaultsToEmptyArray()
    {
        var result = AnalysisResult.Create(_comparisonId, "Summary", "implications", "risk", null!);

        result.KeyChangesJson.Should().Be("[]");
    }
}
