namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageDeletePublisherHandlerTests
{
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly AppConfigDashboard _config = new(
        new PubSubConfig("proj-test", new Dictionary<string, string> { ["FileDeleteRequestedTopic"] = "file-delete-requested" }));

    [Fact]
    public async Task HandleSuccessPathShouldPublishDeleteRequestEvent()
    {
        // Arrange
        var handler = new ImageDeletePublisherHandler(_publisher, _config);
        var request = new FileDeletePublisherRequest("guid-foto.png");

        // Act
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _publisher.Received(1).PublishAsync(
            request,
            "file-delete-requested",
            Arg.Is<IReadOnlyDictionary<string, string>>(d => d["event-type"] == "file-delete-requested"));
    }
}
