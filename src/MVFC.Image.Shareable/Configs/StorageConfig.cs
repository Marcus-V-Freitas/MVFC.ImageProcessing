namespace MVFC.Image.Shareable.Configs;

public sealed record StorageConfig(
    [property: Required] string UploadBucket,
    [property: Required] string ConvertedBucket,
    [property: Required] string ThumbnailBucket,
    [property: Required] string AnalysisBucket);
