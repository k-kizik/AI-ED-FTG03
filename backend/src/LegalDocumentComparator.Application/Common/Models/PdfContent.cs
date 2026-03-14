using LegalDocumentComparator.Domain.ValueObjects;

namespace LegalDocumentComparator.Application.Common.Models;

public class PdfContent
{
    public int PageCount { get; set; }
    public List<PageContent> Pages { get; set; } = new();
    public string FullText { get; set; } = string.Empty;
}

public class PageContent
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<WordInfo> Words { get; set; } = new();
}

public class WordInfo
{
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
