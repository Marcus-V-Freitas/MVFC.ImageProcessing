namespace MVFC.Image.IoC;

public static class AppUploadDependencies
{
    public static async Task RegisterUploadServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.Get<AppConfigUpload>() ?? throw new InvalidOperationException("Upload configuration section is missing.");

        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddScoped<IPublishService, PublishService>();
    }
}