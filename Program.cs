using Anthropic;
using JobAssistant.API.Services;
using Microsoft.Extensions.AI;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);



const string ReactCorsPolicy = "ReactAppPolicy";

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var apiKey = config["ANTHROPIC_API_KEY"];

    if (string.IsNullOrEmpty(apiKey))
        throw new Exception("ANTHROPIC_API_KEY is missing in environment variables");

    var client = new AnthropicClient
    {
        ApiKey = apiKey
    };

    return client.AsIChatClient("claude-sonnet-4-6", 1024);
});
builder.Services.AddSingleton<IClaudeService, ClaudeService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(ReactCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("JobAssistant.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource(PdfService.ActivitySourceName, ClaudeService.ActivitySourceName)
        .AddConsoleExporter());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(ReactCorsPolicy);

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

// Exposes the entry point to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }