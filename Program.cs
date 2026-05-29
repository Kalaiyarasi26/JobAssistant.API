using Anthropic;
using JobAssistant.API.Services;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ANTHROPIC_API_KEY"];

var anthropicClient = new AnthropicClient() { ApiKey = apiKey };

builder.Services.AddSingleton(anthropicClient);
builder.Services.AddSingleton<IChatClient>(sp => sp.GetRequiredService<AnthropicClient>().AsIChatClient("claude-sonnet-4-6", 1024));
builder.Services.AddSingleton<IClaudeService, ClaudeService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/test", async (IChatClient client, CancellationToken cancellationToken) =>
{
    var response = await client.GetResponseAsync(
        new[] { new ChatMessage(ChatRole.User, "Say hello from Job Assistant!") },
        new ChatOptions { MaxOutputTokens = 1024 },
        cancellationToken);

    return response.Text;
});

app.Run();