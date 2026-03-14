using FluentAssertions;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.UseCases.Documents.GetVersionFile;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Exceptions;
using Moq;

namespace LegalDocumentComparator.UnitTests.Application;

public class GetVersionFileQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _docRepoMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly GetVersionFileQueryHandler _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _versionId = Guid.NewGuid();
    private readonly Guid _documentId = Guid.NewGuid();

    public GetVersionFileQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _storageServiceMock.Setup(s => s.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());

        _sut = new GetVersionFileQueryHandler(_docRepoMock.Object, _storageServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsValidationException()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

        var act = () => _sut.Handle(new GetVersionFileQuery { VersionId = _versionId }, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task Handle_WithNonExistentVersion_ThrowsNotFoundException()
    {
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentVersion?)null);

        var act = () => _sut.Handle(new GetVersionFileQuery { VersionId = _versionId }, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_OwnerCanDownloadFile()
    {
        SetupVersion(_userId);

        var result = await _sut.Handle(new GetVersionFileQuery { VersionId = _versionId }, default);

        result.FileStream.Should().NotBeNull();
        result.FileName.Should().Be("contract.pdf");
    }

    [Fact]
    public async Task Handle_ManagerCanDownloadAnyFile()
    {
        var otherUserId = Guid.NewGuid();
        SetupVersion(otherUserId);
        _currentUserMock.Setup(u => u.IsManager).Returns(true);

        var act = () => _sut.Handle(new GetVersionFileQuery { VersionId = _versionId }, default);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_NonOwnerNonManager_ThrowsValidationException()
    {
        var otherUserId = Guid.NewGuid();
        var version = SetupVersion(otherUserId);
        _currentUserMock.Setup(u => u.IsManager).Returns(false);

        var ownerDoc = Document.Create(otherUserId, "Someone Else's Doc");
        _docRepoMock.Setup(r => r.GetByIdAsync(version.DocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerDoc);

        var act = () => _sut.Handle(new GetVersionFileQuery { VersionId = _versionId }, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*permission*");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectFileName()
    {
        _currentUserMock.Setup(u => u.IsManager).Returns(true);
        SetupVersion(_userId);

        var result = await _sut.Handle(new GetVersionFileQuery { VersionId = _versionId }, default);

        result.FileName.Should().Be("contract.pdf");
    }

    private DocumentVersion SetupVersion(Guid docOwnerUserId)
    {
        var version = DocumentVersion.Create(_documentId, "contract.pdf", "/storage/contract.pdf", 5, 1024, "1.0");
        _docRepoMock.Setup(r => r.GetVersionByIdAsync(_versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var doc = Document.Create(docOwnerUserId, "Contract");
        _docRepoMock.Setup(r => r.GetByIdAsync(version.DocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        return version;
    }
}
