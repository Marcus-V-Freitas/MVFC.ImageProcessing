namespace MVFC.Image.IoC;

public static class AppThumbnailDependencies
{
    public static async Task RegisterThumbnailServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigThumbnail>();
        Validator.ValidateObject(appConfig, new ValidationContext(appConfig), validateAllProperties: true);

        services.AddSingleton(appConfig);
        services.AddSingleton(Options.Create(appConfig));

        services.AddMediatorSpecificHandlers(typeof(ImageThumbnailHandler));
        await services.AddStorageServiceDependenciesAsync();
    }
}