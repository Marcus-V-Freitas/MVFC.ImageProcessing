namespace MVFC.Image.Shareable.Requests;

public sealed record PubSubRequest
{
    public PubSubMessageRequest Message { get; set; } = default!;

    public string Subscription { get; set; } = string.Empty;
}