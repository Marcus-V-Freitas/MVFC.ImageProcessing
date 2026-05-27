namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigAnalysis(
    [property: Required] string VisualApiUrl,
    [property: Required] StorageConfig StorageConfig,
    [property: Required] PubSubConfig PubSubConfig);