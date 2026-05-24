namespace MVFC.ImageConverter.Worker.Tests;

public sealed class ConverterWorkerTests
{
    [Fact]
    public async Task GetRootShouldReturnOk()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new ConverterWorkerFactory(mockMediator);

        // Act
        var response = await factory.GetRootAsync();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("mvfc-image-converter-worker ok");
    }

    [Fact]
    public async Task PostPubSubPushShouldReturnOkWhenRequestIsValid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileUploadedRequest, Result>(Arg.Is<FileUploadedRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Ok()));
        using var factory = new ConverterWorkerFactory(mockMediator);

        var payload = new FileUploadedRequest("test.png", "image/png", 100, "uploads", DateTime.UtcNow);
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        var pubsubRequest = new PubSubRequest(
            new PubSubMessageRequest(base64Data, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );

        // Act
        var response = await factory.PostPubSubPushAsync(pubsubRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostPubSubPushShouldReturnBadRequestWhenDataIsEmpty()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new ConverterWorkerFactory(mockMediator);

        var emptyRequest = new PubSubRequest(
            new PubSubMessageRequest("", "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );

        // Act
        var response = await factory.PostPubSubPushAsync(emptyRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostPubSubPushAndLoggerEnabledShouldLogWarningAndReturnBadRequestWhenRequestIsInvalid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        var mockLogger = Substitute.For<ILogger<Program>>();
        mockLogger.IsEnabled(LogLevel.Warning).Returns(true);

        using var factory = new ConverterWorkerFactory(mockMediator, mockLogger);

        var nullBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("null"));
        var invalidRequest = new PubSubRequest(
            new PubSubMessageRequest(nullBase64, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );

        // Act
        var response = await factory.PostPubSubPushAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PostPubSubPushAndLoggerDisabledShouldNotLogWarningAndReturnBadRequestWhenRequestIsInvalid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        var mockLogger = Substitute.For<ILogger<Program>>();
        mockLogger.IsEnabled(LogLevel.Warning).Returns(false);

        using var factory = new ConverterWorkerFactory(mockMediator, mockLogger);

        var nullBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("null"));
        var invalidRequest = new PubSubRequest(
            new PubSubMessageRequest(nullBase64, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );

        // Act
        var response = await factory.PostPubSubPushAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        mockLogger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PostPubSubPushShouldReturnUnprocessableEntityWhenMediatorFails()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileUploadedRequest, Result>(Arg.Is<FileUploadedRequest>(r => r.FileName == "fail.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Fail("Error occurred")));
        using var factory = new ConverterWorkerFactory(mockMediator);

        var failPayload = new FileUploadedRequest("fail.png", "image/png", 100, "uploads", DateTime.UtcNow);
        var failBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(failPayload)));
        var failRequest = new PubSubRequest(
            new PubSubMessageRequest(failBase64, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );

        // Act
        var response = await factory.PostPubSubPushAsync(failRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
