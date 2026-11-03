using JobAssistant.API.Controllers;
using JobAssistant.API.Models;
using JobAssistant.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobAssistant.API.Tests;

public class AnalyzeControllerTests
{
    private readonly FakeClaudeService _claude = new("Fake advice");
    private readonly FakePdfService _pdf = new("text from pdf");
    private readonly AnalyzeController _controller;

    public AnalyzeControllerTests()
    {
        _controller = new AnalyzeController(_claude, _pdf);
    }

    // ---- POST /api/analyze (JSON) ----

    [Fact]
    public async Task Post_NullRequest_ReturnsBadRequest()
    {
        var result = await _controller.Post(null!, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, _claude.CallCount);
    }

    [Theory]
    [InlineData("", "the job")]
    [InlineData("   ", "the job")]
    [InlineData("my resume", "")]
    [InlineData("my resume", "   ")]
    public async Task Post_BlankResumeOrJobDescription_ReturnsBadRequestAndSkipsClaude(string resume, string job)
    {
        var request = new AnalyzeRequest { ResumeText = resume, JobDescription = job };

        var result = await _controller.Post(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, _claude.CallCount);
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsOkWithClaudeAdvice()
    {
        var request = new AnalyzeRequest { ResumeText = "my resume", JobDescription = "the job" };

        var result = await _controller.Post(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<AnalyzeResponse>(ok.Value);
        Assert.Equal("Fake advice", body.Advice);
    }

    [Fact]
    public async Task Post_ValidRequest_PassesResumeAndJobDescriptionToClaudeService()
    {
        var request = new AnalyzeRequest { ResumeText = "my resume", JobDescription = "the job" };

        await _controller.Post(request, CancellationToken.None);

        Assert.Equal(1, _claude.CallCount);
        Assert.Equal("my resume", _claude.LastResumeText);
        Assert.Equal("the job", _claude.LastJobDescription);
    }

    // ---- POST /api/analyze/upload (PDF) ----

    [Fact]
    public async Task Upload_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.Upload(null!, "the job", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, _pdf.CallCount);
        Assert.Equal(0, _claude.CallCount);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        var result = await _controller.Upload(ToFormFile(Array.Empty<byte>()), "the job", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, _pdf.CallCount);
        Assert.Equal(0, _claude.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Upload_BlankJobDescription_ReturnsBadRequest(string job)
    {
        var result = await _controller.Upload(ToFormFile(new byte[] { 1, 2, 3 }), job, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, _pdf.CallCount);
        Assert.Equal(0, _claude.CallCount);
    }

    [Fact]
    public async Task Upload_ValidFile_ReturnsOkWithClaudeAdvice()
    {
        var result = await _controller.Upload(ToFormFile(new byte[] { 1, 2, 3 }), "the job", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<AnalyzeResponse>(ok.Value);
        Assert.Equal("Fake advice", body.Advice);
    }

    [Fact]
    public async Task Upload_ValidFile_SendsExtractedPdfTextAndJobDescriptionToClaudeService()
    {
        var file = ToFormFile(new byte[] { 1, 2, 3 });

        await _controller.Upload(file, "the job", CancellationToken.None);

        Assert.Same(file, _pdf.LastFile);
        Assert.Equal(1, _claude.CallCount);
        Assert.Equal("text from pdf", _claude.LastResumeText);
        Assert.Equal("the job", _claude.LastJobDescription);
    }

    private static FormFile ToFormFile(byte[] bytes)
    {
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "resumePdf", "resume.pdf");
    }

    // Stand-in for ClaudeService: returns canned advice and records what it was given.
    private sealed class FakeClaudeService : IClaudeService
    {
        private readonly string _advice;

        public FakeClaudeService(string advice)
        {
            _advice = advice;
        }

        public int CallCount { get; private set; }
        public string? LastResumeText { get; private set; }
        public string? LastJobDescription { get; private set; }

        public Task<string> AnalyzeCareerAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastResumeText = resumeText;
            LastJobDescription = jobDescription;
            return Task.FromResult(_advice);
        }
    }

    // Stand-in for PdfService: returns canned text instead of parsing a real PDF.
    private sealed class FakePdfService : IPdfService
    {
        private readonly string _text;

        public FakePdfService(string text)
        {
            _text = text;
        }

        public int CallCount { get; private set; }
        public IFormFile? LastFile { get; private set; }

        public Task<string> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastFile = file;
            return Task.FromResult(_text);
        }
    }
}
