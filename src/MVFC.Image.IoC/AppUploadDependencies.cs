namespace MVFC.Image.IoC;

public static class AppUploadDependencies
{
    public static async Task RegisterUploadServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigUpload>();

        services.AddMediatorSpecificHandlers(typeof(ImageUploadHandler));
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddSingleton<IPublisherClientFactory, PublisherClientFactory>();
        services.AddScoped<IPublishService, PublishService>();
    }
}