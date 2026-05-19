var builder = WebApplication.CreateBuilder(args);

await builder.Services.RegisterThumbnailServicesAsync(builder.Configuration);

var app = builder.Build();

app.MapPost("/pubsub/push", async (PubSubRequest request, IMediator mediator, CancellationToken ct) =>
{
    var bytes = Convert.FromBase64String(request.Message.Data);
    var json = Encoding.UTF8.GetString(bytes);
    var evt = JsonSerializer.Deserialize<FileThumbnailRequest>(json)!;

    await mediator.Send<FileThumbnailRequest, Result>(evt, ct);

    return Results.Ok();
});

app.MapGet("/", () => "mvfc-image-thumbnail-worker ok");

await app.RunAsync();