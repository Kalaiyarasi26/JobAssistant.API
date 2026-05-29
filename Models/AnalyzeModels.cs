namespace JobAssistant.API.Models;

public sealed class AnalyzeRequest
{
    public string ResumeText { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
}

public sealed class AnalyzeResponse
{
    public string Advice { get; set; } = string.Empty;
}
