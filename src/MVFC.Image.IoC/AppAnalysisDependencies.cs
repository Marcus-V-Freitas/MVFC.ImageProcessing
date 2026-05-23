namespace MVFC.Image.IoC;

public static class AppAnalysisDependencies
{
    public static async Task RegisterAnalysisServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigAnalysis>();

        services.AddMediatorSpecificHandlers(typeof(ImageAnalysisHandler));
        services.AddSingleton(appConfig);
        await services.AddStorageServiceDependenciesAsync();

        services.AddRefitClient<IVisionApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(appConfig.VisualApiUrl))
                .AddStandardResilienceHandler();
    }
}