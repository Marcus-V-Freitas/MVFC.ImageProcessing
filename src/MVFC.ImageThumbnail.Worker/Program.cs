var builder = WebApplication.CreateBuilder(args);

var storage = await new StorageClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
}
.BuildAsync();

var publisher = await new PublisherClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    TopicName = TopicName.FromProjectTopic("local-project", "thumbnail-created-topic")
}
.BuildAsync();

builder.Services.AddSingleton(storage);
builder.Services.AddSingleton(publisher);
builder.Services.AddScoped<ThumbnailService>();

var app = builder.Build();

app.MapPost("/pubsub/push", async (PubSubRequest request, ThumbnailService service, CancellationToken ct) =>
{
    var bytes = Convert.FromBase64String(request.Message.Data);
    var json = Encoding.UTF8.GetString(bytes);
    var evt = JsonSerializer.Deserialize<FileUploadedRequest>(json)!;

    await service.ProcessAsync(evt, ct);

    return Results.Ok();
});

app.MapGet("/", () => "mvfc-image-thumbnail-worker ok");

await app.RunAsync();