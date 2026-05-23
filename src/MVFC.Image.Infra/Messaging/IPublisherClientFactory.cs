namespace MVFC.Image.Infra.Messaging;

public interface IPublisherClientFactory
{
    Task<PublisherClient> CreateAsync(string projectId, string topicId);
}
