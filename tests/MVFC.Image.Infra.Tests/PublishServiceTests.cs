namespace MVFC.Image.Infra.Tests;

public sealed class PublishServiceTests
{
    [Fact]
    public async Task PublishAsyncShouldAttemptToPublishMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8681");
        
        var mockPublisher = Substitute.For<PublisherClient>();
        mockPublisher.PublishAsync(Arg.Is<PubsubMessage>(m => m.Attributes["key"] == "value"))
                     .Returns(Task.FromResult("test-message-id"));

        var mockFactory = Substitute.For<IPublisherClientFactory>();
        mockFactory.CreateAsync(Arg.Is("test-project"), Arg.Is("test-topic"))
                   .Returns(Task.FromResult(mockPublisher));

        var config = new PubSubConfig("test-project", new Dictionary<string, string>());
        var service = new PublishService(config, mockFactory);

        var message = new { Text = "test" };
        var attributes = new Dictionary<string, string> { { "key", "value" } };

        // Act & Assert
        await service.Invoking(s => s.PublishAsync(message, "test-topic", attributes).AsTask())
                     .Should().NotThrowAsync();

        await mockPublisher.Received(1).PublishAsync(Arg.Is<PubsubMessage>(m => m.Attributes["key"] == "value"));
    }
}
