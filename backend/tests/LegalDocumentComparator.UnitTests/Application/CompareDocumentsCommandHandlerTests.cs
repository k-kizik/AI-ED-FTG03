using System.Text.Json;
using FluentAssertions;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.Common.Models;
using LegalDocumentComparator.Application.UseCases.Comparisons.CompareDocuments;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;
using LegalDocumentComparator.Domain.Exceptions;
using LegalDocumentComparator.Domain.ValueObjects;
using Moq;

namespace LegalDocumentComparator.UnitTests.Application;

public class CompareDocumentsCommandHandlerTests
{
    private readonly Mock<IComparisonRepository> _compRepoMock = new();
    private readonly Mock<IDocumentRepository> _docRepoMock = new();
    private readonly Mock<IPdfService> _pdfServiceMock = new();
    private readonly Mock<IAiService> _aiServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CompareDocumentsCommandHandler _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _documentId = Guid.NewGuid();
    private readonly Guid _origVersionId = Guid.NewGuid();
    private readonly Guid _newVersionId = Guid.NewGuid();

    public CompareDocumentsCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);

        _pdfServiceMock.Setup(p => p.CompareDocumentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TextDifference>
            {
                new() { Type = ChangeType.Modified, PageNumber = 1, OldText = "old", NewText = "new", Position = new TextPosition(1, 0, 0, 10, 10) }
            });

        _pdfServiceMock.Setup(p => p.ExtractTextWithLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PdfContent { PageCount = 2, FullText = "some text" });

        _aiServiceMock.Setup(a => a.AnalyzeDocumentChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ChangeDetail>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComparisonAnalysis
            {
                Summary = "Summary",
                LegalImplications = "Implications",
                RiskAssessment = "Medium",
                KeyChanges = new List<KeyChange>
                {
                    new() { Title = "Key Change", Description = "Desc", Impact = "Impact", Severity = "High", Recommendation = "Review" }
                }
            });

        _sut = new CompareDocumentsCommandHandler(
            _compRepoMock.Object,
            _docRepoMock.Object,
            _pdfServiceMock.Object,
            _aiServiceMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsValidationException()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

        var act = () => _sut.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task Handle_WithNonExistentOriginalVersion_ThrowsNotFoundException()
    {
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_origVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentVersion?)null);

        var act = () => _sut.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentNewVersion_ThrowsNotFoundException()
    {
        SetupMatchingVersions();
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_newVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentVersion?)null);

        var act = () => _sut.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithVersionsFromDifferentDocuments_ThrowsValidationException()
    {
        var origVersion = DocumentVersion.Create(_documentId, "v1.pdf", "/v1.pdf", 2, 100, "1.0");
        var differentDocId = Guid.NewGuid();
        var newVersion = DocumentVersion.Create(differentDocId, "v2.pdf", "/v2.pdf", 2, 100, "2.0");

        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_origVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(origVersion);
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_newVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newVersion);

        var act = () => _sut.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*same document*");
    }

    [Fact]
    public async Task Handle_WithExistingCompletedComparison_ReturnsCachedResult()
    {
        SetupMatchingVersions();
        var existing = CreateCompletedComparison();
        _compRepoMock.Setup(r => r.GetByVersionIdsAsync(_origVersionId, _newVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(new CompareDocumentsCommand
        {
            OriginalVersionId = _origVersionId,
            NewVersionId = _newVersionId,
            ForceRegenerate = false
        }, default);

        result.WasGenerated.Should().BeFalse();
        result.ComparisonId.Should().Be(existing.Id);
        _aiServiceMock.Verify(a => a.AnalyzeDocumentChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ChangeDetail>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithForceRegenerate_BypassesCache()
    {
        SetupMatchingVersions();
        var existing = CreateCompletedComparison();
        _compRepoMock.Setup(r => r.GetByVersionIdsAsync(_origVersionId, _newVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(new CompareDocumentsCommand
        {
            OriginalVersionId = _origVersionId,
            NewVersionId = _newVersionId,
            ForceRegenerate = true
        }, default);

        result.WasGenerated.Should().BeTrue();
        _aiServiceMock.Verify(a => a.AnalyzeDocumentChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ChangeDetail>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewComparison_CallsAiServiceAndReturnsResult()
    {
        SetupMatchingVersions();
        _compRepoMock.Setup(r => r.GetByVersionIdsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comparison?)null);

        var result = await _sut.Handle(ValidCommand(), default);

        result.WasGenerated.Should().BeTrue();
        result.Summary.Should().Be("Summary");
        result.LegalImplications.Should().Be("Implications");
        result.RiskAssessment.Should().Be("Medium");
        result.Changes.Should().HaveCount(1);
        result.Changes[0].Title.Should().Be("Key Change");
    }

    [Fact]
    public async Task Handle_NewComparison_PersistsAnalysisResultAndMarksCompleted()
    {
        SetupMatchingVersions();
        _compRepoMock.Setup(r => r.GetByVersionIdsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comparison?)null);

        await _sut.Handle(ValidCommand(), default);

        _compRepoMock.Verify(r => r.AddAnalysisResultAsync(It.IsAny<AnalysisResult>(), It.IsAny<CancellationToken>()), Times.Once);
        _compRepoMock.Verify(r => r.UpdateAsync(It.Is<Comparison>(c => c.Status == ComparisonStatus.Completed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAiServiceThrows_MarksComparisonAsFailed()
    {
        SetupMatchingVersions();
        _compRepoMock.Setup(r => r.GetByVersionIdsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comparison?)null);
        _aiServiceMock.Setup(a => a.AnalyzeDocumentChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ChangeDetail>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI unavailable"));

        var act = () => _sut.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _compRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Comparison>(c => c.Status == ComparisonStatus.Failed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingNonCompletedComparison_ReprocessesIt()
    {
        SetupMatchingVersions();
        var existing = Comparison.Create(_origVersionId, _newVersionId, _userId);
        existing.MarkAsProcessing();
        existing.MarkAsFailed("previous error");
        _compRepoMock.Setup(r => r.GetByVersionIdsAsync(_origVersionId, _newVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(new CompareDocumentsCommand
        {
            OriginalVersionId = _origVersionId,
            NewVersionId = _newVersionId,
        }, default);

        result.WasGenerated.Should().BeTrue();
        _compRepoMock.Verify(r => r.AddAsync(It.IsAny<Comparison>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private CompareDocumentsCommand ValidCommand() => new()
    {
        OriginalVersionId = _origVersionId,
        NewVersionId = _newVersionId
    };

    private void SetupMatchingVersions()
    {
        var orig = DocumentVersion.Create(_documentId, "v1.pdf", "/v1.pdf", 2, 100, "1.0");
        var next = DocumentVersion.Create(_documentId, "v2.pdf", "/v2.pdf", 2, 100, "2.0");
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_origVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orig);
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_newVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(next);
    }

    private Comparison CreateCompletedComparison()
    {
        var comparison = Comparison.Create(_origVersionId, _newVersionId, _userId);
        comparison.MarkAsProcessing();
        comparison.MarkAsCompleted();
        // Use reflection to set AnalysisResult since it has private setter
        var analysisResult = AnalysisResult.Create(comparison.Id, "Cached summary", "Cached implications", "Low", JsonSerializer.Serialize(new List<KeyChange>()));
        typeof(Comparison).GetProperty("AnalysisResult")!
            .SetValue(comparison, analysisResult);
        return comparison;
    }
}
