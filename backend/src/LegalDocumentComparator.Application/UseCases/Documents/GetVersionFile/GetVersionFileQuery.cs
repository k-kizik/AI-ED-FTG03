namespace LegalDocumentComparator.Application.UseCases.Documents.GetVersionFile;

public class GetVersionFileQuery
{
    public Guid VersionId { get; set; }
}

public class GetVersionFileResult
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
