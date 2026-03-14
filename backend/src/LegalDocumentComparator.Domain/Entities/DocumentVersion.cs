namespace LegalDocumentComparator.Domain.Entities;

public class DocumentVersion
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string FilePath { get; private set; } = null!;
    public string VersionNumber { get; private set; } = null!;
    public int PageCount { get; private set; }
    public long FileSizeBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public Document Document { get; private set; } = null!;
    public ICollection<Comparison> OriginalComparisons { get; private set; } = new List<Comparison>();
    public ICollection<Comparison> NewComparisons { get; private set; } = new List<Comparison>();

    private DocumentVersion() { }

    public static DocumentVersion Create(
        Guid documentId,
        string fileName,
        string filePath,
        int pageCount,
        long fileSizeBytes,
        string versionNumber)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        if (pageCount <= 0)
            throw new ArgumentException("Page count must be positive", nameof(pageCount));

        if (pageCount > 1000)
            throw new ArgumentException("Page count cannot exceed 1000", nameof(pageCount));

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSizeBytes));

        if (string.IsNullOrWhiteSpace(versionNumber))
            throw new ArgumentException("Version number cannot be empty", nameof(versionNumber));

        return new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            FileName = fileName,
            FilePath = filePath,
            VersionNumber = versionNumber,
            PageCount = pageCount,
            FileSizeBytes = fileSizeBytes,
            UploadedAt = DateTime.UtcNow
        };
    }
}
