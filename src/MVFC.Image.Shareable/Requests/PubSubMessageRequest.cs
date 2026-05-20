namespace MVFC.Image.Shareable.Requests;

public sealed record PubSubMessageRequest(
    string Data,
    string MessageId,
    string PublishTime,
    IReadOnlyDictionary<string, string>? Attributes = null);