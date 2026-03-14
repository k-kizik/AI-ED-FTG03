using LegalDocumentComparator.Application.Common.Models;

namespace LegalDocumentComparator.Application.Common.Interfaces.Services;

public interface IAiService
{
    Task<ComparisonAnalysis> AnalyzeDocumentChangesAsync(
        string originalText,
        string newText,
        List<ChangeDetail> changes,
        CancellationToken cancellationToken = default);
}
