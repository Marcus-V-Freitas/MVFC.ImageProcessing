namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageDeleteHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();

    [Fact]
    public async Task Handle_ShouldDeleteAllThreeArtifactsInParallel()
    {
        // Arrange
        var storageConfig = new StorageConfig("uploads", "thumbnails", "analysis-results");
        var handler = new ImageDeleteHandler(_storage, storageConfig);
        var request = new FileDeleteRequest("guid-foto.png");

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _storage.Received(1).DeleteImageAsync("uploads", "guid-foto.png", default);
        await _storage.Received(1).DeleteImageAsync("thumbnails", "thumb-guid-foto.png", default);
        await _storage.Received(1).DeleteImageAsync("analysis-results", "analysis-guid-foto.png.json", default);
    }
}
