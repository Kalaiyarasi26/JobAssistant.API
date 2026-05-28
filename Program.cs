using Anthropic;
using Anthropic.Models.Messages;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ANTHROPIC_API_KEY"];

var anthropicClient = new AnthropicClient() { ApiKey = apiKey };

builder.Services.AddSingleton(anthropicClient);
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/test", async (AnthropicClient client) =>
{
    var message = await client.Messages.Create(new MessageCreateParams()
    {
        Model = "claude-sonnet-4-6",
        MaxTokens = 1024,
        Messages = [
            new() { Role = Role.User, Content = "Say hello from Job Assistant!" }
        ]
    });

    return message.ToString();
});

app.Run();