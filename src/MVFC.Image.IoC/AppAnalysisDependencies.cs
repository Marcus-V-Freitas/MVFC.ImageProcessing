namespace MVFC.Image.IoC;

public static class AppAnalysisDependencies
{
    public static async Task RegisterAnalysisServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigAnalysis>();
        await Validator.ValidateObjectAsync(appConfig, new ValidationContext(appConfig), validateAllProperties: true);

        services.AddSingleton(appConfig);
        services.AddSingleton(Options.Create(appConfig));

        services.AddMediatorSpecificHandlers(typeof(ImageAnalysisHandler));
        await services.AddStorageServiceDependenciesAsync();

        services.AddRefitGeneratedClient<IVisionApiClient>()
                .ConfigureHttpClient((sp, c) =>
                {
                    var appConfig = sp.GetRequiredService<AppConfigAnalysis>();
                    c.BaseAddress = new Uri(appConfig.VisualApiUrl);
                })
                .AddStandardResilienceHandler();
    }
}