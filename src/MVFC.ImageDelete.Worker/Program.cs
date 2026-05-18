var builder = WebApplication.CreateBuilder(args);

var storage = await new StorageClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
}
.BuildAsync();

builder.Services.AddSingleton(storage);
builder.Services.AddScoped<DeleteService>();

var app = builder.Build();

app.MapPost("/pubsub/push", async (PubSubRequest request, DeleteService service, CancellationToken ct) =>
{
    var bytes = Convert.FromBase64String(request.Message.Data);
    var json = Encoding.UTF8.GetString(bytes);
    var evt = JsonSerializer.Deserialize<FileDeleteRequest>(json)!;

    await service.DeleteAsync(evt, ct);

    return Results.Ok();
});

app.MapGet("/", () => "mvfc-image-delete-worker ok");

await app.RunAsync();
