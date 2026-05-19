namespace MVFC.Image.IoC;

public static class AppAnalysisDependencies
{
    public static async Task RegisterAnalysisServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.Get<AppConfigAnalysis>() ?? throw new InvalidOperationException("Analysis configuration section is missing.");

        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        services.AddSingleton(appConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddRefitClient<IVisionApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(appConfig.VisualApiUrl));
    }
}