using FluentAssertions;
using LegalDocumentComparator.Domain.ValueObjects;

namespace LegalDocumentComparator.UnitTests.Domain;

public class TextPositionTests
{
    [Fact]
    public void Constructor_WithValidInputs_SetsProperties()
    {
        var pos = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);

        pos.PageNumber.Should().Be(1);
        pos.X.Should().Be(10.0);
        pos.Y.Should().Be(20.0);
        pos.Width.Should().Be(100.0);
        pos.Height.Should().Be(15.0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidPageNumber_ThrowsArgumentException(int pageNumber)
    {
        var act = () => new TextPosition(pageNumber, 0, 0, 0, 0);

        act.Should().Throw<ArgumentException>().WithParameterName("pageNumber");
    }

    [Fact]
    public void Equals_SamePageAndCoordinates_ReturnsTrue()
    {
        var pos1 = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);
        var pos2 = new TextPosition(1, 10.0, 20.0, 50.0, 5.0); // Width/Height not in Equals

        pos1.Equals(pos2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentPage_ReturnsFalse()
    {
        var pos1 = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);
        var pos2 = new TextPosition(2, 10.0, 20.0, 100.0, 15.0);

        pos1.Equals(pos2).Should().BeFalse();
    }

    [Fact]
    public void Equals_CoordinatesWithinTolerance_ReturnsTrue()
    {
        var pos1 = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);
        var pos2 = new TextPosition(1, 10.005, 20.005, 100.0, 15.0); // within 0.01 tolerance

        pos1.Equals(pos2).Should().BeTrue();
    }

    [Fact]
    public void Equals_CoordinatesOutsideTolerance_ReturnsFalse()
    {
        var pos1 = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);
        var pos2 = new TextPosition(1, 10.02, 20.0, 100.0, 15.0); // outside 0.01 tolerance

        pos1.Equals(pos2).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var pos = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);

        pos.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var pos = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);

        pos.Equals("not a TextPosition").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualPositions_ReturnSameHash()
    {
        var pos1 = new TextPosition(1, 10.0, 20.0, 100.0, 15.0);
        var pos2 = new TextPosition(1, 10.0, 20.0, 50.0, 5.0);

        pos1.GetHashCode().Should().Be(pos2.GetHashCode());
    }
}
