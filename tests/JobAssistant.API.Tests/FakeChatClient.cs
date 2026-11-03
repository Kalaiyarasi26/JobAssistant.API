using Microsoft.Extensions.AI;

namespace JobAssistant.API.Tests;

// A stand-in for the real Claude client. It never touches the network:
// it returns the text we give it and remembers what it was sent.
public sealed class FakeChatClient : IChatClient
{
    private readonly string _replyText;

    public FakeChatClient(string replyText)
    {
        _replyText = replyText;
    }

    public int CallCount { get; private set; }

    public string? LastPrompt { get; private set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastPrompt = messages.Last().Text;

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, _replyText)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Streaming is not used by ClaudeService.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
