namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigThumbnail(
    [property: Required] StorageConfig StorageConfig);