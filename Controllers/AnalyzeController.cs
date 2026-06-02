using JobAssistant.API.Models;
using JobAssistant.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly IClaudeService _claudeService;
    private readonly IPdfService _pdfService;

    public AnalyzeController(IClaudeService claudeService, IPdfService pdfService)
    {
        _claudeService = claudeService;
        _pdfService = pdfService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AnalyzeRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ResumeText) || string.IsNullOrWhiteSpace(request.JobDescription))
        {
            return BadRequest("Both resumeText and jobDescription are required.");
        }

        var analysis = await _claudeService.AnalyzeCareerAsync(request.ResumeText, request.JobDescription, cancellationToken);
        return Ok(new AnalyzeResponse { Advice = analysis });
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile resumePdf, [FromForm] string jobDescription, CancellationToken cancellationToken)
    {
        if (resumePdf is null || resumePdf.Length == 0)
        {
            return BadRequest("A non-empty resume PDF file is required.");
        }

        if (string.IsNullOrWhiteSpace(jobDescription))
        {
            return BadRequest("The jobDescription form field is required.");
        }

        var resumeText = await _pdfService.ExtractTextAsync(resumePdf, cancellationToken);
        var analysis = await _claudeService.AnalyzeCareerAsync(resumeText, jobDescription, cancellationToken);
        return Ok(new AnalyzeResponse { Advice = analysis });
    }
}
