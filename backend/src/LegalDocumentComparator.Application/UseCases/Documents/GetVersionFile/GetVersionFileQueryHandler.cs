using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Exceptions;

namespace LegalDocumentComparator.Application.UseCases.Documents.GetVersionFile;

public class GetVersionFileQueryHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;

    public GetVersionFileQueryHandler(
        IDocumentRepository documentRepository,
        IStorageService storageService,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _storageService = storageService;
        _currentUserService = currentUserService;
    }

    public async Task<GetVersionFileResult> Handle(GetVersionFileQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new ValidationException("User is not authenticated");

        var version = await _documentRepository.GetVersionByIdAsync(query.VersionId, cancellationToken);
        if (version == null)
            throw new NotFoundException(nameof(DocumentVersion), query.VersionId);

        if (!_currentUserService.IsManager)
        {
            var document = await _documentRepository.GetByIdAsync(version.DocumentId, cancellationToken);
            if (document == null || document.UserId != _currentUserService.UserId)
                throw new ValidationException("You do not have permission to access this file");
        }

        var stream = await _storageService.GetFileAsync(version.FilePath, cancellationToken);

        return new GetVersionFileResult
        {
            FileStream = stream,
            FileName = version.FileName
        };
    }
}
