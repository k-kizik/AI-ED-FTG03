using FluentAssertions;
using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.UnitTests.Domain;

public class DocumentTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_ReturnsDocumentWithCorrectProperties()
    {
        var doc = Document.Create(_userId, "My Contract", "A description");

        doc.Id.Should().NotBeEmpty();
        doc.UserId.Should().Be(_userId);
        doc.Name.Should().Be("My Contract");
        doc.Description.Should().Be("A description");
        doc.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        doc.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithoutDescription_DefaultsToEmpty()
    {
        var doc = Document.Create(_userId, "Contract");

        doc.Description.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var act = () => Document.Create(_userId, name!);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Update_WithValidName_UpdatesNameAndTimestamp()
    {
        var doc = Document.Create(_userId, "Old Name", "Old Desc");
        var before = doc.UpdatedAt;

        doc.Update("New Name", "New Desc");

        doc.Name.Should().Be("New Name");
        doc.Description.Should().Be("New Desc");
        doc.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var doc = Document.Create(_userId, "Contract");

        var act = () => doc.Update(name!, "desc");

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var doc1 = Document.Create(_userId, "Contract A");
        var doc2 = Document.Create(_userId, "Contract B");

        doc1.Id.Should().NotBe(doc2.Id);
    }
}
