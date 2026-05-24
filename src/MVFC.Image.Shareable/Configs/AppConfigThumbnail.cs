namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigThumbnail(
    [property: Required] PubSubConfig PubSubConfig,
    [property: Required] StorageConfig StorageConfig);