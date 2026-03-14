using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.Application.Common.Interfaces.Repositories;

public interface IComparisonRepository
{
    Task<Comparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Comparison?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Comparison?> GetByVersionIdsAsync(Guid originalVersionId, Guid newVersionId, CancellationToken cancellationToken = default);
    Task<List<Comparison>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Comparison>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Comparison comparison, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comparison comparison, CancellationToken cancellationToken = default);
    Task AddChangesAsync(IEnumerable<Change> changes, CancellationToken cancellationToken = default);
    Task AddAnalysisResultAsync(AnalysisResult analysisResult, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comparison comparison, CancellationToken cancellationToken = default);
}
