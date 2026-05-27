namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageDeletePublisherHandlerTests
{
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly ILogger<ImageDeletePublisherHandler> _logger = Substitute.For<ILogger<ImageDeletePublisherHandler>>();
    private readonly AppConfigDashboard _config = new(
        new PubSubConfig("proj-test", "image-upload", "file-converted", "thumbnail-created", "analysis-completed-topic", "file-delete-requested"));
    private readonly ImageDeletePublisherHandler _sut;

    public ImageDeletePublisherHandlerTests() =>
        _sut = new(_publisher, _config, _logger);

    [Fact]
    public async Task HandleSuccessPathShouldPublishDeleteRequestEvent()
    {
        // Arrange
        var request = new FileDeletePublisherRequest("guid-foto.png");

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _publisher.Received(1).PublishAsync(
            request,
            "file-delete-requested",
            Arg.Is<IReadOnlyDictionary<string, string>>(d => d["event-type"] == "file-delete-requested"));
    }

    [Fact]
    public async Task HandleWhenPublisherThrowsAndLoggerEnabledShouldLogAndReturnFail()
    {
        // Arrange
        var request = new FileDeletePublisherRequest("guid-foto.png");
        var exception = new InvalidOperationException("PubSub unavailable");

        _logger.IsEnabled(LogLevel.Error)
               .Returns(true);

        _publisher.When(x => x.PublishAsync(
            Arg.Any<FileDeletePublisherRequest>(),
            "file-delete-requested",
            Arg.Any<IReadOnlyDictionary<string, string>>()))
            .Do(_ => throw exception);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message == "PubSub unavailable");
        
        _logger.Received(1).LogErrorDeletePublish(exception, request.FileName);
    }
}
