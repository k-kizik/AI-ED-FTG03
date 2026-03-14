using LegalDocumentComparator.Application.UseCases.Comparisons.CompareDocuments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocumentComparator.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ComparisonsController : ControllerBase
{
    private readonly CompareDocumentsCommandHandler _compareDocumentsHandler;

    public ComparisonsController(CompareDocumentsCommandHandler compareDocumentsHandler)
    {
        _compareDocumentsHandler = compareDocumentsHandler;
    }

    /// <summary>
    /// Compare two document versions and get AI-powered analysis
    /// </summary>
    /// <param name="command">Version IDs to compare</param>
    /// <returns>Detailed comparison with AI analysis of changes</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/comparisons/compare
    ///     {
    ///         "originalVersionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "newVersionId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///         "forceRegenerate": false
    ///     }
    /// 
    /// The response includes:
    /// - Detailed list of all textual changes
    /// - AI-generated summary and legal implications
    /// - Risk assessment (Low, Medium, High, Critical)
    /// - Key changes with recommendations
    /// - Text positions for highlighting in UI
    /// 
    /// **Note**: First comparison takes longer (AI analysis). Subsequent requests use cached results unless forceRegenerate=true.
    /// 
    /// **Processing time**: 2-5 seconds (Groq Cloud)
    /// </remarks>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(CompareDocumentsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompareDocumentsResult>> CompareDocuments([FromBody] CompareDocumentsCommand command)
    {
        var result = await _compareDocumentsHandler.Handle(command, CancellationToken.None);
        return Ok(result);
    }
}

