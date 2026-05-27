var builder = WebApplication.CreateBuilder(args);

await builder.Services.RegisterThumbnailServicesAsync(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapPost("/pubsub/push", async (
    PubSubRequest request,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Message.Data))
        return Results.BadRequest("Payload vazio.");

    var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Message.Data));
    var evt = JsonSerializer.Deserialize<FileThumbnailRequest>(json);

    if (evt is null)
    {
        logger.LogWarningInvalidPayload(request.Message.Data);
        return Results.BadRequest("Deserialização falhou.");
    }

    var result = await mediator.Send<FileThumbnailRequest, Result>(evt, ct);

    return result.IsSuccess
        ? Results.Ok()
        : Results.UnprocessableEntity(result.Errors);
});

app.MapHealthChecks("/health");
app.MapGet("/", () => "mvfc-image-thumbnail-worker ok");

await app.RunAsync();