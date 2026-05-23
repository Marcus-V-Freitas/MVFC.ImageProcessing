namespace MVFC.Image.IoC;

public static class AppDeleteDependencies
{
    public static async Task RegisterDeleteServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var storageConfig = configuration.GetRequiredConfig<StorageConfig>("StorageConfig");
        services.AddSingleton(storageConfig);
        services.AddMediatorSpecificHandlers(typeof(ImageDeleteHandler));
        await services.AddStorageServiceDependenciesAsync();
    }
}