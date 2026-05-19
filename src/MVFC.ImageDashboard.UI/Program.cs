var builder = WebApplication.CreateBuilder(args);
await builder.Services.RegisterDashboardServicesAsync(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/files", async (IMediator mediator, CancellationToken cancellationToken) =>
{
    var result = await mediator.Send<FileGalleryRequest, Result<FileGalleryResponse>>(new(), cancellationToken);
    return Results.Ok(result.Value);
});

app.MapPost("/api/delete/{fileName}", async (string fileName, IMediator mediator, CancellationToken cancellationToken) =>
{
    await mediator.Send<FileDeletePublisherRequest, Result>(new(fileName), cancellationToken);
    return Results.Accepted();
});

await app.RunAsync();
