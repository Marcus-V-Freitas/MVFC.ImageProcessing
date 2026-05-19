namespace MVFC.Image.Shareable.Configs;

public sealed record PubSubConfig(
    string ProjectId,
    IReadOnlyDictionary<string, string> Topics);