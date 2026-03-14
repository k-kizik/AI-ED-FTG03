using LegalDocumentComparator.Application.Common.Models;

namespace LegalDocumentComparator.Application.Common.Interfaces.Services;

public interface IPdfService
{
    Task<PdfContent> ExtractTextWithLayoutAsync(string filePath, CancellationToken cancellationToken = default);
    Task<List<TextDifference>> CompareDocumentsAsync(string originalPath, string newPath, CancellationToken cancellationToken = default);
}
