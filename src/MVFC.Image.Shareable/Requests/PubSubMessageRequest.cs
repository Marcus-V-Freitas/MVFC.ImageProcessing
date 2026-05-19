namespace MVFC.Image.Shareable.Requests;

public sealed record PubSubMessageRequest(
    string Data,
    string MessageId,
    string PublishTime)
{
    public IDictionary<string, string>? Attributes { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}