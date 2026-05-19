namespace MVFC.Image.Infra;

public static class StorageServiceDependencies
{
    public static async Task AddStorageServiceDependenciesAsync(this IServiceCollection services)
    {
        services.AddSingleton<StorageClient>(await CreateStorageClientAsync());
        services.AddScoped<IStorageService, StorageService>();
    }

    private static async Task<StorageClient> CreateStorageClientAsync()
    {
        return await new StorageClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync();
    }
}