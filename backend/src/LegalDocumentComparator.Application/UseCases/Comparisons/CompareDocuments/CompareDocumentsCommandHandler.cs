using System.Text.Json;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.Common.Models;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;
using LegalDocumentComparator.Domain.Exceptions;

namespace LegalDocumentComparator.Application.UseCases.Comparisons.CompareDocuments;

public class CompareDocumentsCommandHandler
{
    private readonly IComparisonRepository _comparisonRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IPdfService _pdfService;
    private readonly IAiService _aiService;
    private readonly ICurrentUserService _currentUserService;

    public CompareDocumentsCommandHandler(
        IComparisonRepository comparisonRepository,
        IDocumentRepository documentRepository,
        IPdfService pdfService,
        IAiService aiService,
        ICurrentUserService currentUserService)
    {
        _comparisonRepository = comparisonRepository;
        _documentRepository = documentRepository;
        _pdfService = pdfService;
        _aiService = aiService;
        _currentUserService = currentUserService;
    }

    public async Task<CompareDocumentsResult> Handle(CompareDocumentsCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new ValidationException("User is not authenticated");

        var originalVersion = await _documentRepository.GetVersionByIdAsync(command.OriginalVersionId, cancellationToken);
        if (originalVersion == null)
            throw new NotFoundException(nameof(DocumentVersion), command.OriginalVersionId);

        var newVersion = await _documentRepository.GetVersionByIdAsync(command.NewVersionId, cancellationToken);
        if (newVersion == null)
            throw new NotFoundException(nameof(DocumentVersion), command.NewVersionId);

        if (originalVersion.DocumentId != newVersion.DocumentId)
            throw new ValidationException("Versions must belong to the same document");

        var existingComparison = await _comparisonRepository.GetByVersionIdsAsync(
            command.OriginalVersionId, 
            command.NewVersionId, 
            cancellationToken);

        if (existingComparison != null && existingComparison.Status == ComparisonStatus.Completed && !command.ForceRegenerate)
        {
            return MapToResult(existingComparison, wasGenerated: false);
        }

        Comparison comparison;
        if (existingComparison != null)
        {
            comparison = existingComparison;
            comparison.MarkAsProcessing();
        }
        else
        {
            comparison = Comparison.Create(
                command.OriginalVersionId,
                command.NewVersionId,
                _currentUserService.UserId!.Value);
            await _comparisonRepository.AddAsync(comparison, cancellationToken);
        }

        try
        {
            var differences = await _pdfService.CompareDocumentsAsync(
                originalVersion.FilePath,
                newVersion.FilePath,
                cancellationToken);

            var changeDetails = differences.Select(d => new ChangeDetail
            {
                Type = d.Type,
                Severity = DetermineSeverity(d),
                PageNumber = d.PageNumber,
                OldText = d.OldText,
                NewText = d.NewText,
                Description = GenerateDescription(d),
                LegalMeaning = string.Empty
            }).ToList();

            var originalContent = await _pdfService.ExtractTextWithLayoutAsync(originalVersion.FilePath, cancellationToken);
            var newContent = await _pdfService.ExtractTextWithLayoutAsync(newVersion.FilePath, cancellationToken);

            var analysis = await _aiService.AnalyzeDocumentChangesAsync(
                originalContent.FullText,
                newContent.FullText,
                changeDetails,
                cancellationToken);

            var analysisResult = AnalysisResult.Create(
                comparison.Id,
                analysis.Summary,
                analysis.LegalImplications,
                analysis.RiskAssessment,
                JsonSerializer.Serialize(analysis.KeyChanges));

            await _comparisonRepository.AddAnalysisResultAsync(analysisResult, cancellationToken);

            comparison.MarkAsCompleted();
            await _comparisonRepository.UpdateAsync(comparison, cancellationToken);

            return MapNewResult(comparison.Id, analysisResult, analysis);
        }
        catch (Exception ex)
        {
            comparison.MarkAsFailed(ex.Message);
            await _comparisonRepository.UpdateAsync(comparison, cancellationToken);
            throw;
        }
    }

    private ChangeSeverity DetermineSeverity(TextDifference diff)
    {
        var textLength = Math.Max(diff.OldText.Length, diff.NewText.Length);
        
        if (textLength > 500) return ChangeSeverity.High;
        if (textLength > 200) return ChangeSeverity.Medium;
        return ChangeSeverity.Low;
    }

    private string GenerateDescription(TextDifference diff)
    {
        return diff.Type switch
        {
            ChangeType.Added => $"Added text on page {diff.PageNumber}",
            ChangeType.Deleted => $"Deleted text on page {diff.PageNumber}",
            ChangeType.Modified => $"Modified text on page {diff.PageNumber}",
            _ => $"Change on page {diff.PageNumber}"
        };
    }

    private CompareDocumentsResult MapToResult(Comparison comparison, bool wasGenerated)
    {
        var keyChanges = new List<KeyChangeDto>();
        if (comparison.AnalysisResult?.KeyChangesJson is not null)
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<KeyChange>>(comparison.AnalysisResult.KeyChangesJson);
                keyChanges = parsed?.Select(k => new KeyChangeDto
                {
                    Title = k.Title,
                    Description = k.Description,
                    Impact = k.Impact,
                    Severity = k.Severity,
                    Recommendation = k.Recommendation
                }).ToList() ?? new();
            }
            catch { /* ignore malformed JSON */ }
        }

        return new CompareDocumentsResult
        {
            ComparisonId = comparison.Id,
            Summary = comparison.AnalysisResult?.Summary ?? string.Empty,
            LegalImplications = comparison.AnalysisResult?.LegalImplications ?? string.Empty,
            RiskAssessment = comparison.AnalysisResult?.RiskAssessment ?? string.Empty,
            Changes = keyChanges,
            WasGenerated = wasGenerated
        };
    }

    private CompareDocumentsResult MapNewResult(Guid comparisonId, AnalysisResult analysisResult, ComparisonAnalysis analysis)
    {
        var keyChanges = analysis.KeyChanges.Select(k => new KeyChangeDto
        {
            Title = k.Title,
            Description = k.Description,
            Impact = k.Impact,
            Severity = k.Severity,
            Recommendation = k.Recommendation
        }).ToList();

        return new CompareDocumentsResult
        {
            ComparisonId = comparisonId,
            Summary = analysisResult.Summary,
            LegalImplications = analysisResult.LegalImplications,
            RiskAssessment = analysisResult.RiskAssessment,
            Changes = keyChanges,
            WasGenerated = true
        };
    }
}
