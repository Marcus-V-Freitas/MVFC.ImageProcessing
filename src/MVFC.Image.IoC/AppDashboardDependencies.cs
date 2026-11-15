namespace MVFC.Image.IoC;

public static class AppDashboardDependencies
{
    public static async Task RegisterDashboardServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigDashboard>();
        await Validator.ValidateObjectAsync(appConfig, new ValidationContext(appConfig), validateAllProperties: true);

        services.AddSingleton(appConfig);
        services.AddSingleton(Options.Create(appConfig));
        services.AddSingleton(appConfig.PubSubConfig);

        services.AddMediatorSpecificHandlers(typeof(ImageGalleryHandler), typeof(ImageDeletePublisherHandler));
        await services.AddStorageServiceDependenciesAsync();

        services.AddSingleton<IPublisherClientFactory, PublisherClientFactory>();
        services.AddScoped<IPublishService, PublishService>();
    }
}