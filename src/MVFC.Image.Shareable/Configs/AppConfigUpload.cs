namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigUpload(
    [property: Required] StorageConfig StorageConfig);