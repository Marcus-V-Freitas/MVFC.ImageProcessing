var builder = WebApplication.CreateBuilder(args);

await builder.Services.RegisterDashboardServicesAsync(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<SseClientManager>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");

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

app.MapGet("/events/stream", async (SseClientManager sse, HttpContext context, CancellationToken cancellationToken) =>
{
    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Connection = "keep-alive";
    await context.Response.Body.FlushAsync(cancellationToken);

    var channel = sse.AddClient();

    try
    {
        await foreach (var eventType in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await context.Response.WriteAsync($"event: {eventType}\ndata: refresh\n\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);
        }
    }
    catch (OperationCanceledException)
    {
        // Client disconnected
    }
    finally
    {
        sse.RemoveClient(channel);
    }
});

app.MapPost("/pubsub/notify", (SseClientManager sse) =>
{
    sse.Broadcast("gallery-updated");
    return Results.Ok();
});

await app.RunAsync();
