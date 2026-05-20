namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageAnalysisHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IVisionApiClient _visionClient = Substitute.For<IVisionApiClient>();
    private readonly ILogger<ImageAnalysisHandler> _logger = Substitute.For<ILogger<ImageAnalysisHandler>>();
    private readonly AppConfigAnalysis _config = new("http://vision-api/test", new StorageConfig("uploads", "thumbnails", "analysis-results"));

    [Fact]
    public async Task Handle_SuccessPath_ShouldDownloadAnalyzeAndUploadResult()
    {
        // Arrange
        var handler = new ImageAnalysisHandler(_storage, _visionClient, _config, _logger);
        var request = new FileConvertedRequest("foto.png", "image/png", 1024, "uploads", DateTime.UtcNow);

        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var imageStream = new MemoryStream(imageBytes);
        _storage.DownloadImageAsync("uploads", "foto.png", default)
                .Returns(Task.FromResult(imageStream));

        _visionClient.AnalyzeImageAsync(Arg.Is<VisionApiRequest>(r => r.Image == Convert.ToBase64String(imageBytes)), default)
                     .Returns(Task.FromResult("{\"tags\":[\"test\"]}"));

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _storage.Received(1).DownloadImageAsync("uploads", "foto.png", default);
        await _visionClient.Received(1).AnalyzeImageAsync(Arg.Is<VisionApiRequest>(r => r.Image == Convert.ToBase64String(imageBytes)), default);
        await _storage.Received(1).UploadImageAsync(
            "analysis-results",
            "analysis-foto.png.json",
            "application/json",
            Arg.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "{\"tags\":[\"test\"]}"),
            default);
    }

    [Fact]
    public async Task Handle_VisionClientThrowsException_ShouldLogErrorAndReturnFail()
    {
        // Arrange
        var handler = new ImageAnalysisHandler(_storage, _visionClient, _config, _logger);
        var request = new FileConvertedRequest("foto.png", "image/png", 1024, "uploads", DateTime.UtcNow);

        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var imageStream = new MemoryStream(imageBytes);
        _storage.DownloadImageAsync("uploads", "foto.png", default)
                .Returns(Task.FromResult(imageStream));

        var exception = new Exception("Vision API down");
        _visionClient.AnalyzeImageAsync(Arg.Is<VisionApiRequest>(r => r != null), default)
                     .ThrowsAsync(exception);

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message == "Vision API down");
        
        // Verify logger was called
        _logger.ReceivedCalls().Should().NotBeEmpty();
    }
}
