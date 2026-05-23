namespace MVFC.Image.Infra.Messaging;

public sealed class PublisherClientFactory : IPublisherClientFactory
{
    public Task<PublisherClient> CreateAsync(string projectId, string topicId)
    {
        var topicName = TopicName.FromProjectTopic(projectId, topicId);

        return new PublisherClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            TopicName = topicName,
        }.BuildAsync();
    }
}
