namespace MVFC.Image.IoC;

public static class AppUploadDependencies
{
    public static async Task RegisterUploadServicesAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.GetRequiredConfig<AppConfigUpload>();
        Validator.ValidateObject(appConfig, new ValidationContext(appConfig), validateAllProperties: true);

        services.AddSingleton(appConfig);
        services.AddSingleton(Options.Create(appConfig));

        services.AddMediatorSpecificHandlers(typeof(ImageUploadHandler));
        await services.AddStorageServiceDependenciesAsync();
    }
}