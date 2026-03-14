namespace LegalDocumentComparator.Application.UseCases.Documents.GetDocuments;

public class GetDocumentsQuery
{
    public bool IncludeAllUsers { get; set; }
}

public class GetDocumentsResult
{
    public List<DocumentDto> Documents { get; set; } = new();
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DocumentVersionDto> Versions { get; set; } = new();
}

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string VersionNumber { get; set; } = null!;
    public int PageCount { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
