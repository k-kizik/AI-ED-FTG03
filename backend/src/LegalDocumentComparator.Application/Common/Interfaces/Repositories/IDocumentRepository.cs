using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.Application.Common.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Document>> GetAllWithVersionsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Document document, CancellationToken cancellationToken = default);
    Task<DocumentVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task AddVersionAsync(DocumentVersion version, CancellationToken cancellationToken = default);
}
