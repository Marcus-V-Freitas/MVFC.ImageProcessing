namespace MVFC.Image.IoC;

public static class AppConverterDependencies
{
    public static async Task RegisterConverterServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigConverter>();
        Validator.ValidateObject(appConfig, new ValidationContext(appConfig), validateAllProperties: true);

        services.AddSingleton(appConfig);
        services.AddSingleton(Options.Create(appConfig));
        services.AddSingleton(appConfig.PubSubConfig);

        services.AddMediatorSpecificHandlers(typeof(ImageConverterHandler));
        await services.AddStorageServiceDependenciesAsync();

        services.AddSingleton<IPublisherClientFactory, PublisherClientFactory>();
        services.AddScoped<IPublishService, PublishService>();
    }
}