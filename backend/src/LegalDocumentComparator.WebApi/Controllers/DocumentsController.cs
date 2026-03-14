using LegalDocumentComparator.Application.UseCases.Documents.GetDocuments;
using LegalDocumentComparator.Application.UseCases.Documents.GetVersionFile;
using LegalDocumentComparator.Application.UseCases.Documents.UploadDocument;
using LegalDocumentComparator.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocumentComparator.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly GetDocumentsQueryHandler _getDocumentsHandler;
    private readonly UploadDocumentCommandHandler _uploadDocumentHandler;
    private readonly GetVersionFileQueryHandler _getVersionFileHandler;

    public DocumentsController(
        GetDocumentsQueryHandler getDocumentsHandler,
        UploadDocumentCommandHandler uploadDocumentHandler,
        GetVersionFileQueryHandler getVersionFileHandler)
    {
        _getDocumentsHandler = getDocumentsHandler;
        _uploadDocumentHandler = uploadDocumentHandler;
        _getVersionFileHandler = getVersionFileHandler;
    }

    /// <summary>
    /// Get all documents accessible by the current user
    /// </summary>
    /// <param name="includeAllUsers">Managers only: Include documents from all users</param>
    /// <returns>List of documents with their versions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetDocumentsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetDocumentsResult>> GetDocuments([FromQuery] bool includeAllUsers = false)
    {
        var query = new GetDocumentsQuery { IncludeAllUsers = includeAllUsers };
        var result = await _getDocumentsHandler.Handle(query, CancellationToken.None);
        return Ok(result);
    }

    /// <summary>
    /// Upload a new document or a new version of an existing document
    /// </summary>
    /// <param name="request">Document upload details</param>
    /// <returns>Uploaded document information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/documents/upload
    ///     Content-Type: multipart/form-data
    ///     
    ///     file: [Select a PDF file]
    ///     name: "Service Agreement"
    ///     versionNumber: "1.0"
    ///     description: "Initial draft"
    ///     existingDocumentId: (optional, for adding a new version)
    /// 
    /// </remarks>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadDocumentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(104857600)] // 100 MB limit
    public async Task<ActionResult<UploadDocumentResult>> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("File is required");

        if (!request.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are supported");

        var command = new UploadDocumentCommand
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            VersionNumber = request.VersionNumber,
            FileStream = request.File.OpenReadStream(),
            FileName = request.File.FileName,
            FileSize = request.File.Length,
            ExistingDocumentId = request.ExistingDocumentId
        };

        var result = await _uploadDocumentHandler.Handle(command, CancellationToken.None);
        return Ok(result);
    }

    /// <summary>
    /// Download a specific document version as PDF
    /// </summary>
    /// <param name="id">Document version ID</param>
    /// <returns>PDF file stream</returns>
    [HttpGet("versions/{id}/file")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersionFile(Guid id)
    {
        var query = new GetVersionFileQuery { VersionId = id };
        var result = await _getVersionFileHandler.Handle(query, CancellationToken.None);
        return File(result.FileStream, "application/pdf", result.FileName);
    }
}

