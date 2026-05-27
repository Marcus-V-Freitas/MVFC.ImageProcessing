namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageDeleteHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly ILogger<ImageDeleteHandler> _logger = Substitute.For<ILogger<ImageDeleteHandler>>();
    private readonly StorageConfig _config = new("uploads", "converted", "thumbnails", "analysis-results");
    private readonly ImageDeleteHandler _sut;

    public ImageDeleteHandlerTests() =>
        _sut = new(_storage, _config, _logger);

    [Fact]
    public async Task HandleShouldDeleteAllArtifactsInParallel()
    {
        // Arrange
        var request = new FileDeleteRequest("guid-foto.png");

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _storage.Received(1).DeleteImageAsync("uploads", "guid-foto.png", TestContext.Current.CancellationToken);
        await _storage.Received(1).DeleteImageAsync("converted", "guid-foto.png", TestContext.Current.CancellationToken);
        await _storage.Received(1).DeleteImageAsync("thumbnails", "thumb-guid-foto.png", TestContext.Current.CancellationToken);
        await _storage.Received(1).DeleteImageAsync("analysis-results", "analysis-guid-foto.png.json", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleWhenStorageThrowsAndLoggerEnabledShouldLogAndReturnFail()
    {
        // Arrange
        var request = new FileDeleteRequest("guid-foto.png");
        var exception = new InvalidOperationException("Storage unavailable");

        _logger.IsEnabled(LogLevel.Error)
               .Returns(true);

        _storage.DeleteImageAsync("uploads", "guid-foto.png", TestContext.Current.CancellationToken)
                .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message == "Storage unavailable");
        
        _logger.Received(1).LogErrorDelete(exception, request.FileName);
    }
}