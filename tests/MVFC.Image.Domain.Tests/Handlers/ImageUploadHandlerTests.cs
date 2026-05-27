namespace MVFC.Image.Domain.Tests.Handlers;

public sealed class ImageUploadHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly AppConfigUpload _config = new(
        new StorageConfig("uploads", "converted", "thumbnails", "analysis-results"));
    private readonly ImageUploadHandler _sut;

    public ImageUploadHandlerTests() =>
        _sut = new(_storage, _config);

    [Fact]
    public async Task HandleSuccessPathShouldUploadToStorage()
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
    }
}
