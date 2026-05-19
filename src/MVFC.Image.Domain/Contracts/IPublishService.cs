namespace MVFC.Image.Domain.Contracts;

public interface IPublishService
{
    ValueTask PublishAsync<T>(T message, string topic, IReadOnlyDictionary<string, string> attributes) where T : class;
}