namespace MVFC.Image.Shareable.Requests;

public sealed record PubSubRequest(
    PubSubMessageRequest Message,
    string Subscription) : ICommand;