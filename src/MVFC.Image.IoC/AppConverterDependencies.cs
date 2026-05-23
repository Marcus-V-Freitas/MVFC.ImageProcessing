namespace MVFC.Image.IoC;

public static class AppConverterDependencies
{
    public static async Task RegisterConverterServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigConverter>();

        services.AddMediatorSpecificHandlers(typeof(ImageConverterHandler));
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddSingleton<IPublisherClientFactory, PublisherClientFactory>();
        services.AddScoped<IPublishService, PublishService>();
    }
}