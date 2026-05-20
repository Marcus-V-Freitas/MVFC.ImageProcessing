namespace MVFC.Image.Shareable.Configs;

public sealed record StorageConfig(
    string UploadBucket,
    string ThumbnailBucket,
    string AnalysisBucket);
