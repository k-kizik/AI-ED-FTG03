using FluentAssertions;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;
using LegalDocumentComparator.Domain.ValueObjects;

namespace LegalDocumentComparator.UnitTests.Domain;

public class ChangeTests
{
    private readonly Guid _comparisonId = Guid.NewGuid();
    private readonly TextPosition _position = new(1, 10.0, 20.0, 100.0, 15.0);

    [Fact]
    public void Create_WithValidInputs_ReturnsChangeWithCorrectProperties()
    {
        var change = Change.Create(
            _comparisonId,
            ChangeType.Modified,
            ChangeSeverity.High,
            1,
            "old text",
            "new text",
            "description",
            "legal meaning",
            _position);

        change.Id.Should().NotBeEmpty();
        change.ComparisonId.Should().Be(_comparisonId);
        change.Type.Should().Be(ChangeType.Modified);
        change.Severity.Should().Be(ChangeSeverity.High);
        change.PageNumber.Should().Be(1);
        change.OldText.Should().Be("old text");
        change.NewText.Should().Be("new text");
        change.Description.Should().Be("description");
        change.LegalMeaning.Should().Be("legal meaning");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidPageNumber_ThrowsArgumentException(int pageNumber)
    {
        var act = () => Change.Create(_comparisonId, ChangeType.Added, ChangeSeverity.Low, pageNumber, "", "", "", "", _position);

        act.Should().Throw<ArgumentException>().WithParameterName("pageNumber");
    }

    [Fact]
    public void Create_WithNullTexts_DefaultsToEmpty()
    {
        var change = Change.Create(_comparisonId, ChangeType.Added, ChangeSeverity.Low, 1, null!, null!, null!, null!, _position);

        change.OldText.Should().BeEmpty();
        change.NewText.Should().BeEmpty();
        change.Description.Should().BeEmpty();
        change.LegalMeaning.Should().BeEmpty();
    }

    [Fact]
    public void GetPosition_ReturnsPositionWithCorrectValues()
    {
        var change = Change.Create(_comparisonId, ChangeType.Modified, ChangeSeverity.Medium, 2, "old", "new", "desc", "legal", new TextPosition(2, 5.0, 10.0, 50.0, 12.0));

        var pos = change.GetPosition();

        pos.PageNumber.Should().Be(2);
        pos.X.Should().Be(5.0);
        pos.Y.Should().Be(10.0);
        pos.Width.Should().Be(50.0);
        pos.Height.Should().Be(12.0);
    }
}
