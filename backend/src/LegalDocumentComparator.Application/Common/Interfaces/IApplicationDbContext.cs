using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
