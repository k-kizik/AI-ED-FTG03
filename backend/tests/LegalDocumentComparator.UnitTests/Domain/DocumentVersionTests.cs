using FluentAssertions;
using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.UnitTests.Domain;

public class DocumentVersionTests
{
    private readonly Guid _documentId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_ReturnsVersionWithCorrectProperties()
    {
        var version = DocumentVersion.Create(_documentId, "contract.pdf", "/storage/contract.pdf", 10, 1024, "1.0");

        version.Id.Should().NotBeEmpty();
        version.DocumentId.Should().Be(_documentId);
        version.FileName.Should().Be("contract.pdf");
        version.FilePath.Should().Be("/storage/contract.pdf");
        version.PageCount.Should().Be(10);
        version.FileSizeBytes.Should().Be(1024);
        version.VersionNumber.Should().Be("1.0");
        version.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFileName_ThrowsArgumentException(string? fileName)
    {
        var act = () => DocumentVersion.Create(_documentId, fileName!, "/path", 1, 100, "1.0");

        act.Should().Throw<ArgumentException>().WithParameterName("fileName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFilePath_ThrowsArgumentException(string? filePath)
    {
        var act = () => DocumentVersion.Create(_documentId, "file.pdf", filePath!, 1, 100, "1.0");

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePageCount_ThrowsArgumentException(int pageCount)
    {
        var act = () => DocumentVersion.Create(_documentId, "file.pdf", "/path", pageCount, 100, "1.0");

        act.Should().Throw<ArgumentException>().WithParameterName("pageCount");
    }

    [Fact]
    public void Create_WithPageCountExceeding1000_ThrowsArgumentException()
    {
        var act = () => DocumentVersion.Create(_documentId, "file.pdf", "/path", 1001, 100, "1.0");

        act.Should().Throw<ArgumentException>().WithParameterName("pageCount");
    }

    [Fact]
    public void Create_WithPageCount1000_Succeeds()
    {
        var version = DocumentVersion.Create(_documentId, "file.pdf", "/path", 1000, 100, "1.0");

        version.PageCount.Should().Be(1000);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveFileSize_ThrowsArgumentException(long fileSize)
    {
        var act = () => DocumentVersion.Create(_documentId, "file.pdf", "/path", 1, fileSize, "1.0");

        act.Should().Throw<ArgumentException>().WithParameterName("fileSizeBytes");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidVersionNumber_ThrowsArgumentException(string? versionNumber)
    {
        var act = () => DocumentVersion.Create(_documentId, "file.pdf", "/path", 1, 100, versionNumber!);

        act.Should().Throw<ArgumentException>().WithParameterName("versionNumber");
    }
}
