namespace MVFC.Image.Infra.Messaging;

public sealed class PublishService(PubSubConfig pubSubConfig, IPublisherClientFactory clientFactory) : IPublishService
{
    private readonly ConcurrentDictionary<string, Lazy<Task<PublisherClient>>> _publishers = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _projectId = pubSubConfig.ProjectId;

    public async ValueTask PublishAsync<T>(T message, string topic, IReadOnlyDictionary<string, string> attributes) where T : class
    {
        var publisher = await GetOrCreatePublisherAsync(topic);

        var pubsubMessage = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(message)),
        };

        foreach (var attr in attributes)
        {
            pubsubMessage.Attributes.Add(attr.Key, attr.Value);
        }

        await publisher.PublishAsync(pubsubMessage);
    }

    private Task<PublisherClient> GetOrCreatePublisherAsync(string topicId) =>
        _publishers.GetOrAdd(
            topicId,
            static (topic, state) => new Lazy<Task<PublisherClient>>(() => state.clientFactory.CreateAsync(state._projectId, topic)),
            (clientFactory, _projectId)).Value;
}
