var builder = WebApplication.CreateBuilder(args);
await builder.Services.RegisterDeleteServicesAsync();

var app = builder.Build();

app.MapPost("/pubsub/push", async (PubSubRequest request, IMediator mediator, CancellationToken ct) =>
{
    var bytes = Convert.FromBase64String(request.Message.Data);
    var json = Encoding.UTF8.GetString(bytes);
    var evt = JsonSerializer.Deserialize<FileDeleteRequest>(json)!;

    await mediator.Send<FileDeleteRequest, Result>(evt, ct);

    return Results.Ok();
});

app.MapGet("/", () => "mvfc-image-delete-worker ok");

await app.RunAsync();
