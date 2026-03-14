using FluentAssertions;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.Common.Models;
using LegalDocumentComparator.Application.UseCases.Documents.UploadDocument;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Exceptions;
using Moq;

namespace LegalDocumentComparator.UnitTests.Application;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _docRepoMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IPdfService> _pdfServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly UploadDocumentCommandHandler _sut;

    private readonly Guid _userId = Guid.NewGuid();

    public UploadDocumentCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _storageServiceMock.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/storage/contract.pdf");
        _pdfServiceMock.Setup(p => p.ExtractTextWithLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PdfContent { PageCount = 5, FullText = "text" });

        _sut = new UploadDocumentCommandHandler(_docRepoMock.Object, _storageServiceMock.Object,
            _pdfServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsValidationException()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

        var act = () => _sut.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task Handle_WithNewDocument_CreatesDocumentAndVersion()
    {
        var result = await _sut.Handle(ValidCommand(), default);

        result.VersionId.Should().NotBeEmpty();
        result.Name.Should().Be("My Contract");
        result.VersionNumber.Should().Be("1.0");
        result.PageCount.Should().Be(5);
        _docRepoMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
        _docRepoMock.Verify(r => r.AddVersionAsync(It.IsAny<DocumentVersion>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingDocument_AddsVersionToExistingDocument()
    {
        var existingDocId = Guid.NewGuid();
        var existing = Document.Create(_userId, "Existing Contract");
        _docRepoMock.Setup(r => r.GetByIdAsync(existingDocId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var cmd = ValidCommand();
        cmd.ExistingDocumentId = existingDocId;

        var result = await _sut.Handle(cmd, default);

        result.DocumentId.Should().Be(existing.Id);
        _docRepoMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _docRepoMock.Verify(r => r.AddVersionAsync(It.IsAny<DocumentVersion>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ThrowsNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _docRepoMock.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var cmd = ValidCommand();
        cmd.ExistingDocumentId = missingId;

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDocumentOwnedByOtherUser_ThrowsValidationException()
    {
        var existingDocId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var existing = Document.Create(otherUserId, "Other's Contract");
        _docRepoMock.Setup(r => r.GetByIdAsync(existingDocId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _currentUserMock.Setup(u => u.IsManager).Returns(false);

        var cmd = ValidCommand();
        cmd.ExistingDocumentId = existingDocId;

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*permission*");
    }

    [Fact]
    public async Task Handle_ManagerCanAddVersionToAnyDocument()
    {
        var existingDocId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var existing = Document.Create(otherUserId, "Other's Contract");
        _docRepoMock.Setup(r => r.GetByIdAsync(existingDocId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _currentUserMock.Setup(u => u.IsManager).Returns(true);

        var cmd = ValidCommand();
        cmd.ExistingDocumentId = existingDocId;

        var act = () => _sut.Handle(cmd, default);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithNullFileStream_ThrowsValidationException()
    {
        var cmd = new UploadDocumentCommand
        {
            Name = "My Contract",
            VersionNumber = "1.0",
            FileName = "contract.pdf",
            FileSize = 10
            // FileStream intentionally omitted — defaults to null!
        };

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("contract.docx")]
    [InlineData("contract.txt")]
    [InlineData("contract")]
    public async Task Handle_WithNonPdfFile_ThrowsValidationException(string fileName)
    {
        var cmd = ValidCommand();
        cmd.FileName = fileName;

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*PDF*");
    }

    [Fact]
    public async Task Handle_WithEmptyDocumentName_ThrowsValidationException()
    {
        var cmd = ValidCommand();
        cmd.Name = "";

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithEmptyVersionNumber_ThrowsValidationException()
    {
        var cmd = ValidCommand();
        cmd.VersionNumber = "";

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static UploadDocumentCommand ValidCommand() => new()
    {
        Name = "My Contract",
        Description = "A test doc",
        VersionNumber = "1.0",
        FileName = "contract.pdf",
        FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
        FileSize = 3
    };
}
