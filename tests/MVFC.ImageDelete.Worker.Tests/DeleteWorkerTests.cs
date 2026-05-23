namespace MVFC.ImageDelete.Worker.Tests;

public sealed class DeleteWorkerTests
{
    [Fact]
    public async Task GetRootShouldReturnOk()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new DeleteWorkerFactory(mockMediator);

        // Act
        var response = await factory.GetRootAsync();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("mvfc-image-delete-worker ok");
    }

    [Fact]
    public async Task PostPubSubPushShouldReturnOkWhenRequestIsValid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileDeleteRequest, Result>(Arg.Is<FileDeleteRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Ok()));
        using var factory = new DeleteWorkerFactory(mockMediator);

        var payload = new FileDeleteRequest("test.png");
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
        using var factory = new DeleteWorkerFactory(mockMediator);

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
    public async Task PostPubSubPushShouldReturnBadRequestWhenRequestIsInvalid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new DeleteWorkerFactory(mockMediator);

        var nullBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("null"));
        var invalidRequest = new PubSubRequest(
            new PubSubMessageRequest(nullBase64, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );

        // Act
        var response = await factory.PostPubSubPushAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostPubSubPushShouldReturnUnprocessableEntityWhenMediatorFails()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileDeleteRequest, Result>(Arg.Is<FileDeleteRequest>(r => r.FileName == "fail.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Fail("Error occurred")));
        using var factory = new DeleteWorkerFactory(mockMediator);

        var failPayload = new FileDeleteRequest("fail.png");
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
