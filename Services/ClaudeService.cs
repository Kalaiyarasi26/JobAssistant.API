using Microsoft.Extensions.AI;
namespace JobAssistant.API.Services;

public interface IClaudeService
{
    Task<string> AnalyzeCareerAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default);
}

public class ClaudeService : IClaudeService
{
    private readonly IChatClient _chatClient;
    private const string ModelName = "claude-sonnet-4-6";

    public ClaudeService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> AnalyzeCareerAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default)
    {
        var prompt = $"You are a career coach. Review the resume and job description below. Provide concise, actionable advice for aligning the resume to the role, highlighting strengths, identifying gaps, and suggesting improvements.\n\nResume:\n{resumeText}\n\nJob Description:\n{jobDescription}\n\nAnswer in a clear, professional way.";

        var messages = new[]
        {
            new ChatMessage(ChatRole.User, prompt)
        };

        var response = await _chatClient.GetResponseAsync(messages, new ChatOptions
        {
            ModelId = ModelName,
            MaxOutputTokens = 1024
        }, cancellationToken);

        return string.IsNullOrWhiteSpace(response.Text) ? string.Empty : response.Text;
    }
}
