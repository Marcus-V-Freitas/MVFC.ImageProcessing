namespace MVFC.ImageDelete.Worker.Tests;

public sealed class DeleteWorkerFactory : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static DeleteWorkerFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("STORAGE_EMULATOR_HOST", "http://localhost:8080");
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "http://localhost:8681");
    }

    public DeleteWorkerFactory(IMediator mediator)
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mediator);
                });
            });
        _client = _factory.CreateClient();
    }

    public async Task<HttpResponseMessage> GetRootAsync()
    {
        return await _client.GetAsync("/");
    }

    public async Task<HttpResponseMessage> PostPubSubPushAsync(PubSubRequest request)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        return await _client.PostAsync("/pubsub/push", content);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
