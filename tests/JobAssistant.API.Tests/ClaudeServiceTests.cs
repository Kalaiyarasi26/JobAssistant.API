using JobAssistant.API.Services;

namespace JobAssistant.API.Tests;

public class ClaudeServiceTests
{
    [Fact]
    public async Task AnalyzeCareerAsync_ReturnsTextFromClaude()
    {
        // Arrange: a fake Claude that always answers "Fake advice"
        var fakeClient = new FakeChatClient("Fake advice");
        var service = new ClaudeService(fakeClient);

        // Act
        var result = await service.AnalyzeCareerAsync("my resume", "the job");

        // Assert
        Assert.Equal("Fake advice", result);
        Assert.Equal(1, fakeClient.CallCount);
    }

    [Fact]
    public async Task AnalyzeCareerAsync_PutsResumeAndJobDescriptionIntoPrompt()
    {
        // Arrange
        var fakeClient = new FakeChatClient("anything");
        var service = new ClaudeService(fakeClient);

        // Act
        await service.AnalyzeCareerAsync("I know C#", "Needs Python");

        // Assert: the fake remembered the prompt it was sent
        Assert.NotNull(fakeClient.LastPrompt);
        Assert.Contains("I know C#", fakeClient.LastPrompt);
        Assert.Contains("Needs Python", fakeClient.LastPrompt);
    }
}
