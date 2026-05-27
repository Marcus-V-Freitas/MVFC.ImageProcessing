namespace MVFC.Image.Shareable.Configs;

public sealed record PubSubConfig(
    [property: Required] string ProjectId,
    [property: Required] string FileDeleteRequestedTopic);