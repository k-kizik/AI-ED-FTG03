using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace LegalDocumentComparator.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IAiProvider _provider;
    private readonly ILogger<AiService> _logger;

    public AiService(IAiProvider provider, ILogger<AiService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<ComparisonAnalysis> AnalyzeDocumentChangesAsync(
        string originalText,
        string newText,
        List<ChangeDetail> changes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Using AI provider: {Provider}", _provider.GetProviderName());

        return await _provider.AnalyzeDocumentChangesAsync(
            originalText,
            newText,
            changes,
            cancellationToken);
    }
}
