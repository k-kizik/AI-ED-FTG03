using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Exceptions;

namespace LegalDocumentComparator.Application.UseCases.Documents.UploadDocument;

public class UploadDocumentCommandHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStorageService _storageService;
    private readonly IPdfService _pdfService;
    private readonly ICurrentUserService _currentUserService;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IStorageService storageService,
        IPdfService pdfService,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _storageService = storageService;
        _pdfService = pdfService;
        _currentUserService = currentUserService;
    }

    public async Task<UploadDocumentResult> Handle(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new ValidationException("User is not authenticated");

        if (command.FileStream == null)
            throw new ValidationException(nameof(command.FileStream), "File is required");

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new ValidationException(nameof(command.FileName), "File name is required");

        if (!command.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(nameof(command.FileName), "Only PDF files are supported");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException(nameof(command.Name), "Document name is required");

        if (string.IsNullOrWhiteSpace(command.VersionNumber))
            throw new ValidationException(nameof(command.VersionNumber), "Version number is required");

        Document document;

        if (command.ExistingDocumentId.HasValue)
        {
            document = await _documentRepository.GetByIdAsync(command.ExistingDocumentId.Value, cancellationToken);

            if (document == null)
                throw new NotFoundException(nameof(Document), command.ExistingDocumentId.Value);

            if (document.UserId != _currentUserService.UserId && !_currentUserService.IsManager)
                throw new ValidationException("You don't have permission to add versions to this document");
        }
        else
        {
            document = Document.Create(_currentUserService.UserId!.Value, command.Name, command.Description);
            await _documentRepository.AddAsync(document, cancellationToken);
        }

        var filePath = await _storageService.SaveFileAsync(command.FileStream, command.FileName, cancellationToken);

        var pdfContent = await _pdfService.ExtractTextWithLayoutAsync(filePath, cancellationToken);

        var version = DocumentVersion.Create(
            document.Id,
            command.FileName,
            filePath,
            pdfContent.PageCount,
            command.FileSize,
            command.VersionNumber);

        await _documentRepository.AddVersionAsync(version, cancellationToken);

        return new UploadDocumentResult
        {
            DocumentId = document.Id,
            VersionId = version.Id,
            Name = document.Name,
            VersionNumber = version.VersionNumber,
            PageCount = version.PageCount
        };
    }
}
