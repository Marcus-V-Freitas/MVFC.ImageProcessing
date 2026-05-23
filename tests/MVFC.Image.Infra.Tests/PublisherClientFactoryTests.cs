namespace MVFC.Image.Infra.Tests;

public sealed class PublisherClientFactoryTests
{
    [Fact]
    public async Task CreateAsyncShouldReturnPublisherClient()
    {
        // Arrange
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8681");
        var factory = new PublisherClientFactory();

        // Act
        var result = await factory.CreateAsync("test-project", "test-topic");

        // Assert
        result.Should().NotBeNull();
        result.TopicName.ProjectId.Should().Be("test-project");
        result.TopicName.TopicId.Should().Be("test-topic");
    }
}
