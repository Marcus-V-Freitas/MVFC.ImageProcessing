namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageUploadHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly ILogger<ImageUploadHandler> _logger = Substitute.For<ILogger<ImageUploadHandler>>();
    private readonly AppConfigUpload _config = new(
        new PubSubConfig("proj-test", "image-upload", "file-converted", "thumbnail-created", "file-delete-requested"),
        new StorageConfig("uploads", "thumbnails", "analysis-results"));
    private readonly ImageUploadHandler _sut;

    public ImageUploadHandlerTests() =>
        _sut = new(_storage, _config, _publisher, _logger);

    [Fact]
    public async Task HandleSuccessPathShouldUploadToStorageAndPublishEvent()
    {
        // Arrange
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF, 0xD8]);

        _storage.UploadImageAsync(
            "uploads",
            Arg.Is<string>(s => s.EndsWith("-foto.jpg")),
            "image/jpeg",
            request.Data,
            TestContext.Current.CancellationToken)
            .Returns(Task.FromResult("uploads/guid-foto.jpg"));

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);
            
        // Assert
        result.IsSuccess.Should().BeTrue();

        await _storage.Received(1).UploadImageAsync(
            "uploads",
            Arg.Is<string>(name => name.EndsWith("-foto.jpg")),
            "image/jpeg",
            request.Data,
            TestContext.Current.CancellationToken);

        await _publisher.Received(1).PublishAsync(
            Arg.Is<FileUploadedRequest>(r => r.FileName.EndsWith("-foto.jpg") && r.ContentType == "image/jpeg" && r.Bucket == "uploads"),
            "image-upload",
            Arg.Is<IReadOnlyDictionary<string, string>>(
                d => d["event-type"] == "file.uploaded"));
    }

    [Fact]
    public async Task HandleWhenPublisherThrowsAndLoggerEnabledShouldLogAndReturnFail()
    {
        // Arrange
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF, 0xD8]);
        var exception = new InvalidOperationException("PubSub unavailable");

        _logger.IsEnabled(LogLevel.Warning)
               .Returns(true);

        _publisher.When(x => x.PublishAsync(
            Arg.Any<FileUploadedRequest>(),
            "image-upload",
            Arg.Any<IReadOnlyDictionary<string, string>>()))
            .Do(_ => throw exception);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Upload succeeded but event publish failed"));
        
        _logger.Received(1).LogWarningUploadPublishFailed(exception, request.FileName);
    }
}