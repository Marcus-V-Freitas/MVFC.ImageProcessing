namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigUpload(
    [property: Required] PubSubConfig PubSubConfig,
    [property: Required] StorageConfig StorageConfig);