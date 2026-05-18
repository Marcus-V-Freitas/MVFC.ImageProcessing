namespace MVFC.Image.Shareable.Requests;

public sealed class PubSubMessageRequest
{
    public string Data { get; set; } = string.Empty;

    public string MessageId { get; set; } = string.Empty;

    public string PublishTime { get; set; } = string.Empty;

    public IDictionary<string, string>? Attributes { get; set; }
}