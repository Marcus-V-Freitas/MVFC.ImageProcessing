namespace MVFC.Image.IoC;

public static class AppThumbnailDependencies
{
    public static async Task RegisterThumbnailServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.Get<AppConfigThumbnail>() ?? throw new InvalidOperationException("Thumbnail configuration section is missing.");

        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddScoped<IPublishService, PublishService>();
    }
}