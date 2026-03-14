using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.Domain.Entities;

public class Comparison
{
    public Guid Id { get; private set; }
    public Guid OriginalVersionId { get; private set; }
    public Guid NewVersionId { get; private set; }
    public Guid UserId { get; private set; }
    public ComparisonStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DocumentVersion OriginalVersion { get; private set; } = null!;
    public DocumentVersion NewVersion { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public ICollection<Change> Changes { get; private set; } = new List<Change>();
    public AnalysisResult? AnalysisResult { get; private set; }

    private Comparison() { }

    public static Comparison Create(Guid originalVersionId, Guid newVersionId, Guid userId)
    {
        return new Comparison
        {
            Id = Guid.NewGuid(),
            OriginalVersionId = originalVersionId,
            NewVersionId = newVersionId,
            UserId = userId,
            Status = ComparisonStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        Status = ComparisonStatus.Processing;
    }

    public void MarkAsCompleted()
    {
        Status = ComparisonStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ComparisonStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

}
