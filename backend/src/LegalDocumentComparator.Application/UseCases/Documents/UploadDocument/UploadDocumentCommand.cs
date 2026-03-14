namespace LegalDocumentComparator.Application.UseCases.Documents.UploadDocument;

public class UploadDocumentCommand
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = null!;
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public long FileSize { get; set; }
    public Guid? ExistingDocumentId { get; set; }
}

public class UploadDocumentResult
{
    public Guid DocumentId { get; set; }
    public Guid VersionId { get; set; }
    public string Name { get; set; } = null!;
    public string VersionNumber { get; set; } = null!;
    public int PageCount { get; set; }
}
