namespace MVFC.ImageDashboard.UI.Tests;

public sealed class DashboardUiFactory : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static DashboardUiFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("STORAGE_EMULATOR_HOST", "http://localhost:8080");
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "http://localhost:8681");
    }

    public DashboardUiFactory(IMediator mediator)
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

    public async Task<HttpResponseMessage> GetFilesAsync()
    {
        return await _client.GetAsync("/api/files");
    }

    public async Task<HttpResponseMessage> PostDeleteAsync(string fileName)
    {
        return await _client.PostAsync($"/api/delete/{fileName}", null);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
