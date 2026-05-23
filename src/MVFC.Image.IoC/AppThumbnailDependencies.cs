namespace MVFC.Image.IoC;

public static class AppThumbnailDependencies
{
    public static async Task RegisterThumbnailServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigThumbnail>();

        services.AddMediatorSpecificHandlers(typeof(ImageThumbnailHandler));
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddScoped<IPublishService, PublishService>();
    }
}