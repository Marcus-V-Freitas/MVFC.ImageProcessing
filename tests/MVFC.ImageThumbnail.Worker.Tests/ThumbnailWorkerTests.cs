namespace MVFC.ImageThumbnail.Worker.Tests;

public sealed class ThumbnailWorkerTests
{
    [Fact]
    public async Task GetRootShouldReturnOk()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new ThumbnailWorkerFactory(mockMediator);

        // Act
        var response = await factory.GetRootAsync();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("mvfc-image-thumbnail-worker ok");
    }

    [Fact]
    public async Task PostPubSubPushShouldReturnOkWhenRequestIsValid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileThumbnailRequest, Result>(Arg.Is<FileThumbnailRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Ok()));
        using var factory = new ThumbnailWorkerFactory(mockMediator);

        var payload = new GcsObjectNotification("test.png", "uploads", "image/png", "100", DateTime.UtcNow);
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
        using var factory = new ThumbnailWorkerFactory(mockMediator);

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

        using var factory = new ThumbnailWorkerFactory(mockMediator, mockLogger);

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

        using var factory = new ThumbnailWorkerFactory(mockMediator, mockLogger);

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
        mockMediator.Send<FileThumbnailRequest, Result>(Arg.Is<FileThumbnailRequest>(r => r.FileName == "fail.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Fail("Error occurred")));
        using var factory = new ThumbnailWorkerFactory(mockMediator);

        var failPayload = new GcsObjectNotification("fail.png", "uploads", "image/png", "100", DateTime.UtcNow);
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
