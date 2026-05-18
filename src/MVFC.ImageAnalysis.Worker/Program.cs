var builder = WebApplication.CreateBuilder(args);

var storage = await new StorageClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
}.BuildAsync();

builder.Services.AddSingleton(storage);
builder.Services.AddRefitClient<IVisionApiClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("VISION_API_URL")!);
        c.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<ImageAnalysisService>();

var app = builder.Build();

app.MapPost("/pubsub/push", async (PubSubRequest request, ImageAnalysisService service, CancellationToken ct) =>
{
    var bytes = Convert.FromBase64String(request.Message.Data);
    var json = Encoding.UTF8.GetString(bytes);
    var evt = JsonSerializer.Deserialize<FileUploadedRequest>(json)!;

    await service.ProcessAsync(evt, ct);

    return Results.Ok();
});

app.MapGet("/", () => "mvfc-image-analysis-worker ok");

await app.RunAsync();
