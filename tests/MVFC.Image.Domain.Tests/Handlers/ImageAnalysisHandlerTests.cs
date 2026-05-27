namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageAnalysisHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly IVisionApiClient _visionClient = Substitute.For<IVisionApiClient>();
    private readonly ILogger<ImageAnalysisHandler> _logger = Substitute.For<ILogger<ImageAnalysisHandler>>();
    private readonly AppConfigAnalysis _config = new("http://vision-api/test", new StorageConfig("uploads", "thumbnails", "analysis-results"), new PubSubConfig("p", "u", "c", "t", "analysis-completed-topic", "d"));
    private readonly ImageAnalysisHandler _sut;

    public ImageAnalysisHandlerTests() =>
        _sut = new(_storage, _publisher, _visionClient, _config, _logger);


    [Fact]
    public async Task HandleSuccessPathShouldDownloadAnalyzeAndUploadResult()
    {
        // Arrange
        var request = new FileConvertedRequest("foto.png", "image/png", 1024, "uploads", DateTime.UtcNow);
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var imageStream = new MemoryStream(imageBytes);

        _storage.DownloadImageAsync("uploads", "foto.png", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult(imageStream));

        _visionClient.AnalyzeImageAsync(Arg.Is<VisionApiRequest>(r => r.Image == Convert.ToBase64String(imageBytes)), TestContext.Current.CancellationToken)
                     .Returns(Task.FromResult("{\"tags\":[\"test\"]}"));

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        await _storage.Received(1).DownloadImageAsync("uploads", "foto.png", TestContext.Current.CancellationToken);
        await _visionClient.Received(1).AnalyzeImageAsync(Arg.Is<VisionApiRequest>(r => r.Image == Convert.ToBase64String(imageBytes)), TestContext.Current.CancellationToken);
        await _storage.Received(1).UploadImageAsync(
            "analysis-results",
            "analysis-foto.png.json",
            "application/json",
            Arg.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "{\"tags\":[\"test\"]}"),
            TestContext.Current.CancellationToken);
            
        await _publisher.Received(1).PublishAsync(
            Arg.Is<AnalysisCompletedRequest>(evt => evt.FileName == "foto.png"),
            "analysis-completed-topic",
            Arg.Is<Dictionary<string, string>>(attr => attr["event-type"] == "analysis.completed"));
    }

    [Fact]
    public async Task HandleVisionClientThrowsExceptionAndLoggerEnabledShouldLogAndReturnFail()
    {
        // Arrange
        var request = new FileConvertedRequest("foto.png", "image/png", 1024, "uploads", DateTime.UtcNow);
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var imageStream = new MemoryStream(imageBytes);
        var exception = new InvalidOperationException("Vision API down");

        _logger.IsEnabled(LogLevel.Error)
               .Returns(true);

        _storage.DownloadImageAsync("uploads", "foto.png", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult(imageStream));
        
        _visionClient.AnalyzeImageAsync(Arg.Is<VisionApiRequest>(r => r != null), TestContext.Current.CancellationToken)
                     .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message == "Vision API down");

        _logger.Received(1).LogErrorAnalyze(exception, exception.Message);
    }
}