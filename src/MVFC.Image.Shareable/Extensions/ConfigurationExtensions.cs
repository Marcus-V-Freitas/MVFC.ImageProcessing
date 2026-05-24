namespace MVFC.Image.Shareable.Extensions;

public static class ConfigurationExtensions
{
    public static T GetRequiredConfig<T>(this IConfiguration configuration) where T : class
    {
        return configuration.Get<T>() ?? throw new InvalidOperationException($"A configuração para {typeof(T).Name} não foi encontrada ou está vazia.");
    }

    public static T GetRequiredConfig<T>(this IConfiguration configuration, string sectionName) where T : class
    {
        return configuration.GetSection(sectionName).Get<T>() ?? throw new InvalidOperationException($"A seção de configuração '{sectionName}' para {typeof(T).Name} não foi encontrada ou está vazia.");
    }
}
