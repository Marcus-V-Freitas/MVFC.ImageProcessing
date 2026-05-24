namespace MVFC.Image.IoC;

public static class AppDeleteDependencies
{
    public static async Task RegisterDeleteServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var storageConfig = configuration.GetRequiredConfig<StorageConfig>("StorageConfig");
        Validator.ValidateObject(storageConfig, new ValidationContext(storageConfig), validateAllProperties: true);

        services.AddSingleton(storageConfig);
        services.AddSingleton(Options.Create(storageConfig));
        services.AddMediatorSpecificHandlers(typeof(ImageDeleteHandler));
        await services.AddStorageServiceDependenciesAsync();
    }
}