var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var publisher = await new PublisherClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    TopicName = TopicName.FromProjectTopic("local-project", "file-uploaded-topic")
}.BuildAsync();

var storage = await new StorageClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
}.BuildAsync();

builder.Services.AddSingleton(publisher);
builder.Services.AddSingleton(storage);
builder.Services.AddScoped<UploadService>();

var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapPost("/upload", async (IFormFile file, UploadService service, CancellationToken ct) =>
{
    var fileName = await service.UploadAsync(file, ct);
    return Results.Accepted(fileName);
}).DisableAntiforgery();

app.MapGet("/", () => "mvfc-image-upload-api ok");

await app.RunAsync();