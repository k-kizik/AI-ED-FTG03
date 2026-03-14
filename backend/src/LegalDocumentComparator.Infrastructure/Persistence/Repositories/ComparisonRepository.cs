using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalDocumentComparator.Infrastructure.Persistence.Repositories;

public class ComparisonRepository : IComparisonRepository
{
    private readonly ApplicationDbContext _context;

    public ComparisonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Comparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Comparison?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .Include(c => c.Changes)
            .Include(c => c.AnalysisResult)
            .Include(c => c.OriginalVersion)
            .Include(c => c.NewVersion)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Comparison?> GetByVersionIdsAsync(
        Guid originalVersionId,
        Guid newVersionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .Include(c => c.Changes)
            .Include(c => c.AnalysisResult)
            .FirstOrDefaultAsync(c => 
                c.OriginalVersionId == originalVersionId && 
                c.NewVersionId == newVersionId,
                cancellationToken);
    }

    public async Task<List<Comparison>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .Include(c => c.OriginalVersion)
            .Include(c => c.NewVersion)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Comparison>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .Include(c => c.OriginalVersion)
            .Include(c => c.NewVersion)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Comparison comparison, CancellationToken cancellationToken = default)
    {
        await _context.Comparisons.AddAsync(comparison, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Comparison comparison, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddChangesAsync(IEnumerable<Change> changes, CancellationToken cancellationToken = default)
    {
        await _context.Changes.AddRangeAsync(changes, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAnalysisResultAsync(AnalysisResult analysisResult, CancellationToken cancellationToken = default)
    {
        await _context.AnalysisResults.AddAsync(analysisResult, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Comparison comparison, CancellationToken cancellationToken = default)
    {
        _context.Comparisons.Remove(comparison);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
