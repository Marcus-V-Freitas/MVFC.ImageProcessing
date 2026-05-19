namespace MVFC.Image.IoC;

public static class AppConverterDependencies
{
    public static async Task RegisterConverterServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.Get<AppConfigConverter>() ?? throw new InvalidOperationException("Converter configuration section is missing.");

        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddScoped<IPublishService, PublishService>();
    }
}