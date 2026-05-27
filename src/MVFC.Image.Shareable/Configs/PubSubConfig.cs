namespace MVFC.Image.Shareable.Configs;

public sealed record PubSubConfig(
    [property: Required] string ProjectId,
    [property: Required] string ImageUploadTopic,
    [property: Required] string FileConvertTopic,
    [property: Required] string ThumbnailCreatedTopic,
    [property: Required] string AnalysisCompletedTopic,
    [property: Required] string FileDeleteRequestedTopic);