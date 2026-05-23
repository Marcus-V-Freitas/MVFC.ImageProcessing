
var builder = WebApplication.CreateBuilder(args);
await builder.Services.RegisterDeleteServicesAsync(builder.Configuration);

var app = builder.Build();

app.MapPost("/pubsub/push", async (
    PubSubRequest request,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Message.Data))
        return Results.BadRequest("Payload vazio.");

    var json = Encoding.UTF8.GetString(Convert.FromBase64String(request.Message.Data));
    var evt = JsonSerializer.Deserialize<FileDeleteRequest>(json);

    if (evt is null)
    {
        logger.LogWarningInvalidPayload(request.Message.Data);
        return Results.BadRequest("Deserialização falhou.");
    }

    var result = await mediator.Send<FileDeleteRequest, Result>(evt, ct);

    return result.IsSuccess
        ? Results.Ok()
        : Results.UnprocessableEntity(result.Errors);
});

app.MapGet("/", () => "mvfc-image-delete-worker ok");

await app.RunAsync();
