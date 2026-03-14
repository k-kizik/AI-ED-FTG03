using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;

namespace LegalDocumentComparator.Application.UseCases.Documents.GetDocuments;

public class GetDocumentsQueryHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDocumentsQueryHandler(
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetDocumentsResult> Handle(GetDocumentsQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new ValidationException("User is not authenticated");

        List<Domain.Entities.Document> documents;

        if (query.IncludeAllUsers && _currentUserService.IsManager)
        {
            documents = await _documentRepository.GetAllWithVersionsAsync(cancellationToken);
        }
        else
        {
            documents = await _documentRepository.GetByUserIdAsync(
                _currentUserService.UserId!.Value,
                cancellationToken);
        }

        var documentDtos = documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            UserId = d.UserId,
            UserEmail = d.User?.Email ?? string.Empty,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            Versions = d.Versions.OrderBy(v => v.UploadedAt).Select(v => new DocumentVersionDto
            {
                Id = v.Id,
                FileName = v.FileName,
                VersionNumber = v.VersionNumber,
                PageCount = v.PageCount,
                FileSizeBytes = v.FileSizeBytes,
                UploadedAt = v.UploadedAt
            }).ToList()
        }).ToList();

        return new GetDocumentsResult
        {
            Documents = documentDtos
        };
    }
}
