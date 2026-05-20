namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageGalleryHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();

    [Fact]
    public async Task Handle_SuccessPath_ShouldReturnFileGalleryResponse()
    {
        // Arrange
        var handler = new ImageGalleryHandler(_storage);
        var request = new FileGalleryRequest();

        var uploadsList = new List<string> { "foto1.jpg", "foto2.jpg" };
        var thumbnailsList = new List<string> { "thumb-foto1.jpg" };
        var analysesList = new List<string> { "analysis-foto1.jpg.json" };

        _storage.ListObjectsAsync("uploads", "", default)
                .Returns(Task.FromResult<IReadOnlyList<string>>(uploadsList));
            
        _storage.ListObjectsAsync("thumbnails", "", default)
                .Returns(Task.FromResult<IReadOnlyList<string>>(thumbnailsList));
            
        _storage.ListObjectsAsync("analysis-results", "", default)
                .Returns(Task.FromResult<IReadOnlyList<string>>(analysesList));

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Uploads.Should().BeEquivalentTo(uploadsList);
        result.Value.Thumbnails.Should().BeEquivalentTo(thumbnailsList);
        result.Value.Analyses.Should().BeEquivalentTo(analysesList);

        await _storage.Received(1).ListObjectsAsync("uploads", "", default);
        await _storage.Received(1).ListObjectsAsync("thumbnails", "", default);
        await _storage.Received(1).ListObjectsAsync("analysis-results", "", default);
    }
}
