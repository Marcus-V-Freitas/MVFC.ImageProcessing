namespace MVFC.Image.IoC;

public static class AppDeleteDependencies
{
    public static async Task RegisterDeleteServicesAsync(this IServiceCollection services)
    {
        services.AddMediator(typeof(IDomainEntrypoint).Assembly, typeof(IShareableEntrypoint).Assembly);
        await services.AddStorageServiceDependenciesAsync();
    }
}