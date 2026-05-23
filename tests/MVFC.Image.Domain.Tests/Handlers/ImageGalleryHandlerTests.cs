namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageGalleryHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();

    [Fact]
    public async Task HandleSuccessPathShouldReturnFileGalleryResponse()
    {
        // Arrange
        var handler = new ImageGalleryHandler(_storage);
        var request = new FileGalleryRequest();

        var uploadsList = new List<string> { "foto1.jpg", "foto2.jpg" };
        var thumbnailsList = new List<string> { "thumb-foto1.jpg" };
        var analysesList = new List<string> { "analysis-foto1.jpg.json" };

        _storage.ListObjectsAsync("uploads", "", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult<IReadOnlyList<string>>(uploadsList));
            
        _storage.ListObjectsAsync("thumbnails", "", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult<IReadOnlyList<string>>(thumbnailsList));
            
        _storage.ListObjectsAsync("analysis-results", "", TestContext.Current.CancellationToken)
                .Returns(Task.FromResult<IReadOnlyList<string>>(analysesList));

        // Act
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Uploads.Should().BeEquivalentTo(uploadsList);
        result.Value.Thumbnails.Should().BeEquivalentTo(thumbnailsList);
        result.Value.Analyses.Should().BeEquivalentTo(analysesList);

        await _storage.Received(1).ListObjectsAsync("uploads", "", TestContext.Current.CancellationToken);
        await _storage.Received(1).ListObjectsAsync("thumbnails", "", TestContext.Current.CancellationToken);
        await _storage.Received(1).ListObjectsAsync("analysis-results", "", TestContext.Current.CancellationToken);
    }
}
