namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigConverter(
    [property: Required] PubSubConfig PubSubConfig,
    [property: Required] StorageConfig StorageConfig);