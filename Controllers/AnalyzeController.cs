using JobAssistant.API.Models;
using JobAssistant.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly IClaudeService _claudeService;

    public AnalyzeController(IClaudeService claudeService)
    {
        _claudeService = claudeService;
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
}
