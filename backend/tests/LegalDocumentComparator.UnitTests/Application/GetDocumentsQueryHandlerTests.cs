using FluentAssertions;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.UseCases.Documents.GetDocuments;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;
using Moq;

namespace LegalDocumentComparator.UnitTests.Application;

public class GetDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _docRepoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly GetDocumentsQueryHandler _sut;

    public GetDocumentsQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _sut = new GetDocumentsQueryHandler(_docRepoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsValidationException()
    {
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

        var act = () => _sut.Handle(new GetDocumentsQuery(), default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task Handle_NormalUser_ReturnsOwnDocuments()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);
        _currentUserMock.Setup(u => u.IsManager).Returns(false);

        var docs = new List<Document>
        {
            CreateDocumentWithVersions(userId, "Contract A")
        };
        _docRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs);

        var result = await _sut.Handle(new GetDocumentsQuery { IncludeAllUsers = false }, default);

        result.Documents.Should().HaveCount(1);
        result.Documents[0].Name.Should().Be("Contract A");
        _docRepoMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _docRepoMock.Verify(r => r.GetAllWithVersionsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ManagerWithIncludeAllUsers_ReturnsAllDocuments()
    {
        _currentUserMock.Setup(u => u.IsManager).Returns(true);

        var docs = new List<Document>
        {
            CreateDocumentWithVersions(Guid.NewGuid(), "Contract A"),
            CreateDocumentWithVersions(Guid.NewGuid(), "Contract B")
        };
        _docRepoMock.Setup(r => r.GetAllWithVersionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs);

        var result = await _sut.Handle(new GetDocumentsQuery { IncludeAllUsers = true }, default);

        result.Documents.Should().HaveCount(2);
        _docRepoMock.Verify(r => r.GetAllWithVersionsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonManagerWithIncludeAllUsers_ReturnsOwnDocumentsOnly()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);
        _currentUserMock.Setup(u => u.IsManager).Returns(false);
        _docRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>());

        await _sut.Handle(new GetDocumentsQuery { IncludeAllUsers = true }, default);

        _docRepoMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _docRepoMock.Verify(r => r.GetAllWithVersionsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MapsVersionsOrderedByUploadTime()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);
        _currentUserMock.Setup(u => u.IsManager).Returns(false);

        var doc = CreateDocumentWithVersions(userId, "Contract");
        _docRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { doc });

        var result = await _sut.Handle(new GetDocumentsQuery(), default);

        result.Documents[0].Versions.Should().BeInAscendingOrder(v => v.UploadedAt);
    }

    private static Document CreateDocumentWithVersions(Guid userId, string name)
    {
        var doc = Document.Create(userId, name);
        return doc;
    }
}
