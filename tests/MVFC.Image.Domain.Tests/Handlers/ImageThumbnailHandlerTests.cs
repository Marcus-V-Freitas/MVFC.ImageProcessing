namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageThumbnailHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly AppConfigThumbnail _config = new(
        new PubSubConfig("proj-test", new Dictionary<string, string> { ["ThumbnailCreatedTopic"] = "thumbnail-created" }),
        new StorageConfig("uploads", "thumbnails", "analysis-results"));

    private static readonly byte[] ValidImageBytes =
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 
        0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00, 
        0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 
        0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x3B
    ];

    [Fact]
    public async Task HandleSuccessPathShouldResizeImageAndPublishEvent()
    {
        // Arrange
        var handler = new ImageThumbnailHandler(_storage, _publisher, _config);
        var request = new FileThumbnailRequest("foto.jpg", "image/jpeg", ValidImageBytes.Length, "uploads", DateTime.UtcNow);

        var originalStream = new MemoryStream(ValidImageBytes);
        _storage.DownloadImageAsync("uploads", "foto.jpg", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult(originalStream));

        // Act
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);
        // Assert
        result.IsSuccess.Should().BeTrue();

        await _storage.Received(1).DownloadImageAsync("uploads", "foto.jpg", TestContext.Current.CancellationToken);

        await _storage.Received(1).UploadImageAsync(
            "thumbnails",
            "thumb-foto.jpg",
            "image/png",
            Arg.Is<byte[]>(b => b != null && b.Length > 0),
            TestContext.Current.CancellationToken);

        await _publisher.Received(1).PublishAsync(
            request,
            "thumbnail-created",
            Arg.Is<IReadOnlyDictionary<string, string>>(d => d["event-type"] == "thumbnail-created"));
    }
}
