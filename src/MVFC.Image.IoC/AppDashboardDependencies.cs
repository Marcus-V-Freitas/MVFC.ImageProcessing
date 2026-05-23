namespace MVFC.Image.IoC;

public static class AppDashboardDependencies
{
    public static async Task RegisterDashboardServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigDashboard>();

        services.AddMediatorSpecificHandlers(typeof(ImageGalleryHandler), typeof(ImageDeletePublisherHandler));
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddScoped<IPublishService, PublishService>();
    }
}