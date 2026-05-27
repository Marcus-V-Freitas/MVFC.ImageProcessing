namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageThumbnailHandlerTests
{
    private static readonly byte[] ValidImageBytes =
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00,
        0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
        0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x3B
    ];

    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly ILogger<ImageThumbnailHandler> _logger = Substitute.For<ILogger<ImageThumbnailHandler>>();
    private readonly AppConfigThumbnail _config = new(
        new StorageConfig("uploads", "converted", "thumbnails", "analysis-results"));
    private readonly ImageThumbnailHandler _sut;

    public ImageThumbnailHandlerTests() =>
        _sut = new(_storage, _config, _logger);

    [Fact]
    public async Task HandleSuccessPathShouldResizeImage()
    {
        // Arrange
        var request = new FileThumbnailRequest("foto.png", "image/png", ValidImageBytes.Length, "uploads", DateTime.UtcNow);

        var originalStream = new MemoryStream(ValidImageBytes);
        _storage.DownloadImageAsync("uploads", "foto.png", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult(originalStream));

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _storage.Received(1).DownloadImageAsync("uploads", "foto.png", TestContext.Current.CancellationToken);

        await _storage.Received(1).UploadImageAsync(
            "thumbnails",
            "thumb-foto.png",
            "image/png",
            Arg.Is<byte[]>(b => b != null && b.Length > 0),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleWhenDownloadThrowsAndLoggerEnabledShouldLogAndReturnFail()
    {
        // Arrange
        var request = new FileThumbnailRequest("foto.jpg", "image/jpeg", 1024, "uploads", DateTime.UtcNow);
        var exception = new InvalidOperationException("Download failed");

        _logger.IsEnabled(LogLevel.Error)
               .Returns(true);

        _storage.DownloadImageAsync("uploads", "foto.jpg", TestContext.Current.CancellationToken)
                .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Download failed");
        
        _logger.Received(1).LogErrorThumbnail(exception, request.FileName);
    }
}
