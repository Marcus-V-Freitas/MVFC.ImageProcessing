namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageConverterHandlerTests
{
    private static readonly byte[] ValidImageBytes =
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00,
        0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
        0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x3B
    ];

    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly ILogger<ImageConverterHandler> _logger = Substitute.For<ILogger<ImageConverterHandler>>();
    private readonly AppConfigConverter _config = new(
        new PubSubConfig("proj-test", "image-upload", "file-converted", "thumbnail-created", "file-delete-requested"),
        new StorageConfig("uploads", "thumbnails", "analysis-results"));
    private readonly ImageConverterHandler _sut;

    public ImageConverterHandlerTests() =>
        _sut = new(_storage, _publisher, _config, _logger);

    [Fact]
    public async Task HandleSuccessPathShouldConvertImageToPngAndPublishEvent()
    {
        // Arrange
        var request = new FileUploadedRequest("foto.jpg", "image/jpeg", ValidImageBytes.Length, "uploads", DateTime.UtcNow);
        var originalStream = new MemoryStream(ValidImageBytes);

        _storage.DownloadImageAsync("uploads", "foto.jpg", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult(originalStream));

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        await _storage.Received(1).DownloadImageAsync("uploads", "foto.jpg", TestContext.Current.CancellationToken);
        
        await _storage.Received(1).UploadImageAsync(
            "uploads",
            "foto.jpg",
            "image/png",
            Arg.Is<byte[]>(b => b != null && b.Length > 0),
            TestContext.Current.CancellationToken);

        await _publisher.Received(1).PublishAsync(
            Arg.Is<FileUploadedRequest>(r => r.FileName == "foto.jpg" && r.ContentType == "image/png" && r.Bucket == "uploads"),
            "file-converted",
            Arg.Is<IReadOnlyDictionary<string, string>>(d => d["event-type"] == "file.png.converted"));
    }

    [Fact]
    public async Task HandleInvalidImageFormatAndLoggerEnabledShouldLogAndReturnFail()
    {
        // Arrange
        var request = new FileUploadedRequest("bad.jpg", "image/jpeg", 4, "uploads", DateTime.UtcNow);
        var exception = new InvalidOperationException("Invalid image format");

        _logger.IsEnabled(LogLevel.Error)
               .Returns(true);

        _storage.DownloadImageAsync("uploads", "bad.jpg", TestContext.Current.CancellationToken)
                .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _logger.Received(1).LogErrorConvert(exception, request.FileName);

        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<FileUploadedRequest>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, string>>());
    }
}