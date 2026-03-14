using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocumentComparator.WebApi.Models;

/// <summary>
/// Document upload request model
/// </summary>
public class UploadDocumentRequest
{
    /// <summary>
    /// PDF file to upload (max 100 MB)
    /// </summary>
    [FromForm(Name = "file")]
    public required IFormFile File { get; set; }

    /// <summary>
    /// Document name (e.g., "Service Agreement", "Employment Contract")
    /// </summary>
    [FromForm(Name = "name")]
    public required string Name { get; set; }

    /// <summary>
    /// Version number (e.g., "1.0", "2.1", "Draft")
    /// </summary>
    [FromForm(Name = "versionNumber")]
    public required string VersionNumber { get; set; }

    /// <summary>
    /// Optional description of this version
    /// </summary>
    [FromForm(Name = "description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional: ID of existing document to add this as a new version
    /// </summary>
    [FromForm(Name = "existingDocumentId")]
    public Guid? ExistingDocumentId { get; set; }
}
