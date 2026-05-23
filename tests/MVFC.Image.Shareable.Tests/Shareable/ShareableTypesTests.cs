namespace MVFC.Image.Shareable.Tests.Shareable;

public sealed class ShareableTypesTests
{
    [Fact]
    public void PubSubRequestShouldHoldPropertiesCorrectly()
    {
        // Arrange
        var attributes = new Dictionary<string, string> { { "key", "value" } };
        var message = new PubSubMessageRequest("data-payload", "msg-123", "2026-05-19T20:00:00Z", attributes);

        // Act
        var request = new PubSubRequest(message, "sub-456");

        // Assert
        request.Subscription.Should().Be("sub-456");
        request.Message.Should().NotBeNull();
        request.Message.Data.Should().Be("data-payload");
        request.Message.MessageId.Should().Be("msg-123");
        request.Message.PublishTime.Should().Be("2026-05-19T20:00:00Z");
        request.Message.Attributes.Should().ContainKey("key");
    }

    [Fact]
    public void LogDefinitionsShouldInvokeLoggerWhenEnabled()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(LogLevel.Error).Returns(true);
        logger.IsEnabled(LogLevel.Warning).Returns(true);
        var exception = new InvalidOperationException("Test Exception");

        // Act
        logger.LogErrorAnalyze(exception, "Analyze failed");
        logger.LogErrorConvert(exception, "foto.jpg");
        logger.LogWarningInvalidPayload("invalid-data");

        // Assert
        var logCalls = logger.ReceivedCalls().Where(c => c.GetMethodInfo().Name == "Log").ToList();
        logCalls.Should().HaveCount(3);

        logCalls[0].GetArguments()[0].Should().Be(LogLevel.Error);
        logCalls[0].GetArguments()[1].Should().BeOfType<EventId>().Which.Id.Should().Be(1);
        logCalls[0].GetArguments()[3].Should().Be(exception);

        logCalls[1].GetArguments()[0].Should().Be(LogLevel.Error);
        logCalls[1].GetArguments()[1].Should().BeOfType<EventId>().Which.Id.Should().Be(2);
        logCalls[1].GetArguments()[3].Should().Be(exception);

        logCalls[2].GetArguments()[0].Should().Be(LogLevel.Warning);
        logCalls[2].GetArguments()[1].Should().BeOfType<EventId>().Which.Id.Should().Be(3);
        logCalls[2].GetArguments()[3].Should().BeNull();
    }

    [Fact]
    public void LogDefinitionsShouldNotInvokeLoggerWhenDisabled()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(LogLevel.Error).Returns(false);
        logger.IsEnabled(LogLevel.Warning).Returns(false);
        var exception = new InvalidOperationException("Test Exception");

        // Act
        logger.LogErrorAnalyze(exception, "Analyze failed");
        logger.LogErrorConvert(exception, "foto.jpg");
        logger.LogWarningInvalidPayload("invalid-data");

        // Assert
        var logCalls = logger.ReceivedCalls().Where(c => c.GetMethodInfo().Name == "Log").ToList();
        logCalls.Should().BeEmpty();
    }
}
