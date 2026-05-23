namespace MVFC.ImageUpload.Api.Tests;

public sealed class UploadApiFactory : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static UploadApiFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("STORAGE_EMULATOR_HOST", "http://localhost:8080");
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "http://localhost:8681");
    }

    public UploadApiFactory(IMediator mediator)
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

    public async Task<HttpResponseMessage> PostUploadAsync(MultipartFormDataContent content)
    {
        return await _client.PostAsync("/upload", content);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
