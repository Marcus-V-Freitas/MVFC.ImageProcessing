namespace MVFC.Image.IoC;

public static class AppDeleteDependencies
{
    public static async Task RegisterDeleteServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var storageConfig = configuration.GetSection("StorageConfig").Get<StorageConfig>() ?? throw new InvalidOperationException("StorageConfig section is missing.");
        services.AddSingleton(storageConfig);
        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        await services.AddStorageServiceDependenciesAsync();
    }
}