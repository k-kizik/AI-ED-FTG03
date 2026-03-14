using FluentAssertions;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.UnitTests.Domain;

public class ComparisonTests
{
    private readonly Guid _origVersionId = Guid.NewGuid();
    private readonly Guid _newVersionId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_SetsInitialStatusToPending()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);

        comparison.Status.Should().Be(ComparisonStatus.Pending);
        comparison.CompletedAt.Should().BeNull();
        comparison.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Create_SetsCorrectIds()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);

        comparison.Id.Should().NotBeEmpty();
        comparison.OriginalVersionId.Should().Be(_origVersionId);
        comparison.NewVersionId.Should().Be(_newVersionId);
        comparison.UserId.Should().Be(_userId);
    }

    [Fact]
    public void MarkAsProcessing_ChangesStatusToProcessing()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);

        comparison.MarkAsProcessing();

        comparison.Status.Should().Be(ComparisonStatus.Processing);
    }

    [Fact]
    public void MarkAsCompleted_ChangesStatusToCompletedAndSetsCompletedAt()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);
        comparison.MarkAsProcessing();

        comparison.MarkAsCompleted();

        comparison.Status.Should().Be(ComparisonStatus.Completed);
        comparison.CompletedAt.Should().NotBeNull();
        comparison.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        comparison.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_ChangesStatusToFailedAndSetsErrorMessage()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);
        comparison.MarkAsProcessing();

        comparison.MarkAsFailed("AI service unavailable");

        comparison.Status.Should().Be(ComparisonStatus.Failed);
        comparison.ErrorMessage.Should().Be("AI service unavailable");
        comparison.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_SetsCreatedAtToNow()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);

        comparison.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
