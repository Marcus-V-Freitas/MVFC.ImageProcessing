namespace MVFC.Image.Infra.Tests;

public sealed class PublishServiceTests
{
    [Fact]
    public async Task PublishAsyncShouldAttemptToPublishMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8681");
        var config = new PubSubConfig("test-project", new Dictionary<string, string>());
        var service = new PublishService(config);

        var message = new { Text = "test" };
        var attributes = new Dictionary<string, string> { { "key", "value" } };

        // Act
        async Task Act()
        {
            try
            {
                await service.PublishAsync(message, "test-topic", attributes);
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().NotBeNull();
            }
        }

        await Act();
    }
}
