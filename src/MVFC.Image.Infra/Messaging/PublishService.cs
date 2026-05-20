namespace MVFC.Image.Infra;

public sealed class PublishService(PubSubConfig pubSubConfig) : IPublishService
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
            static (topic, service) => new Lazy<Task<PublisherClient>>(() =>
            {
                var topicName = TopicName.FromProjectTopic(service._projectId, topic);

                return new PublisherClientBuilder
                {
                    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
                    TopicName = topicName,
                }.BuildAsync();
            }),
            this).Value;
}
