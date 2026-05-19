namespace MVFC.Image.IoC;

public static class AppDashboardDependencies
{
    public static async Task RegisterDashboardServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.Get<AppConfigDashboard>() ?? throw new InvalidOperationException("Dashboard configuration section is missing.");

        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.PubSubConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddScoped<IPublishService, PublishService>();
    }
}