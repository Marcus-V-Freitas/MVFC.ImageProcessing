var builder = WebApplication.CreateBuilder(args);

var storage = await new StorageClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
}.BuildAsync();

var deletePublisher = await new PublisherClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    TopicName = TopicName.FromProjectTopic("local-project", "file-delete-requested-topic")
}.BuildAsync();

builder.Services.AddSingleton(storage);
builder.Services.AddSingleton(deletePublisher);
builder.Services.AddScoped<FileGalleryService>();
builder.Services.AddScoped<FileDeletePublisher>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/files", async (FileGalleryService gallery) =>
{
    var result = await gallery.ListFilesAsync();
    return Results.Ok(result);
});

app.MapPost("/api/delete/{fileName}", async (string fileName, FileDeletePublisher publisher) =>
{
    await publisher.RequestDeleteAsync(fileName);
    return Results.Accepted();
});

await app.RunAsync();
