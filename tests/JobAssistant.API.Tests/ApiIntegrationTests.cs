using System.Net;
using System.Net.Http.Json;
using JobAssistant.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobAssistant.API.Tests;

// Starts the real app in memory (real routing, JSON/form binding, controllers
// and services) and replaces only the Claude client with a fake.
// The "Testing" environment keeps user secrets (which hold the real
// ANTHROPIC_API_KEY on a developer machine) out of the test host.
public class ApiFactory : WebApplicationFactory<Program>
{
    public FakeChatClient FakeClaude { get; } = new("Fake advice");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatClient>();
            services.AddSingleton<IChatClient>(FakeClaude);
        });
    }
}

public class ApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public void App_UsesTheFakeClaudeClient_NotTheRealOne()
    {
        var chatClient = _factory.Services.GetRequiredService<IChatClient>();

        Assert.Same(_factory.FakeClaude, chatClient);
    }

    [Fact]
    public async Task PostAnalyze_ValidJson_Returns200WithAdvice()
    {
        var request = new AnalyzeRequest { ResumeText = "json-resume-marker", JobDescription = "json-job-marker" };

        var response = await _client.PostAsJsonAsync("/api/analyze", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();
        Assert.Equal("Fake advice", body!.Advice);
        Assert.Contains("json-resume-marker", _factory.FakeClaude.LastPrompt);
        Assert.Contains("json-job-marker", _factory.FakeClaude.LastPrompt);
    }

    [Fact]
    public async Task PostAnalyze_BlankResume_Returns400()
    {
        var request = new AnalyzeRequest { ResumeText = " ", JobDescription = "the job" };

        var response = await _client.PostAsJsonAsync("/api/analyze", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostAnalyze_NoBody_Returns400()
    {
        var response = await _client.PostAsync("/api/analyze", content: null);

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnsupportedMediaType,
            $"Expected a 4xx client error but got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task PostUpload_PdfAndJobDescription_Returns200AndPdfTextReachesClaude()
    {
        using var form = BuildUploadForm(TestPdf.Create("PdfMarkerText"), "upload-job-marker");

        var response = await _client.PostAsync("/api/analyze/upload", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();
        Assert.Equal("Fake advice", body!.Advice);
        Assert.Contains("PdfMarkerText", _factory.FakeClaude.LastPrompt);
        Assert.Contains("upload-job-marker", _factory.FakeClaude.LastPrompt);
    }

    [Fact]
    public async Task PostUpload_MissingJobDescription_Returns400()
    {
        using var form = new MultipartFormDataContent();
        form.Add(PdfContent(TestPdf.Create("anything")), "resumePdf", "resume.pdf");

        var response = await _client.PostAsync("/api/analyze/upload", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUpload_MissingFile_Returns400()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("the job"), "jobDescription");

        var response = await _client.PostAsync("/api/analyze/upload", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTest_Returns200WithFakeClaudeText()
    {
        var response = await _client.GetAsync("/test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Fake advice", await response.Content.ReadAsStringAsync());
    }

    private static MultipartFormDataContent BuildUploadForm(byte[] pdf, string jobDescription)
    {
        var form = new MultipartFormDataContent();
        form.Add(PdfContent(pdf), "resumePdf", "resume.pdf");
        form.Add(new StringContent(jobDescription), "jobDescription");
        return form;
    }

    private static ByteArrayContent PdfContent(byte[] pdf)
    {
        var content = new ByteArrayContent(pdf);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        return content;
    }
}
