
var builder = WebApplication.CreateBuilder(args);

await builder.Services.RegisterConverterServicesAsync(builder.Configuration);

var app = builder.Build();

app.MapPost("/pubsub/push", async (
    PubSubRequest request,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Message?.Data))
        return Results.BadRequest("Payload vazio.");

    var json = Encoding.UTF8.GetString(Convert.FromBase64String(request.Message.Data));
    var evt = JsonSerializer.Deserialize<FileUploadedRequest>(json);

    if (evt is null)
    {
        logger.LogWarningInvalidPayload(request.Message.Data);
        return Results.BadRequest("Deserialização falhou.");
    }

    var result = await mediator.Send<FileUploadedRequest, Result>(evt, ct);

    return result.IsSuccess
        ? Results.Ok()
        : Results.UnprocessableEntity(result.Errors);
});

app.MapGet("/", () => "mvfc-image-converter-worker ok");

await app.RunAsync();
