namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigConverter(
    [property: Required] StorageConfig StorageConfig);