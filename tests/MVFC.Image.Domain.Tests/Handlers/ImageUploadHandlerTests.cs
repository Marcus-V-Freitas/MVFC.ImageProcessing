namespace MVFC.Image.Domain.Tests.Handlers;

public class ImageUploadHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly AppConfigUpload _config = new(
        new PubSubConfig("proj-test",
            new Dictionary<string, string> { ["ImageUploadTopic"] = "image-upload" }),
        new StorageConfig("uploads", "thumbnails", "analysis-results"));

    [Fact]
    public async Task Handle_ValidRequest_ShouldUploadToStorageAndPublishEvent()
    {
        // Arrange
        var handler = new ImageUploadHandler(_storage, _config, _publisher);
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF, 0xD8]);

        _storage.UploadImageAsync(
            "uploads",
            Arg.Is<string>(s => s.EndsWith("-foto.jpg")),
            "image/jpeg",
            request.Data,
            default)
            .Returns(Task.FromResult("uploads/guid-foto.jpg"));

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _storage.Received(1).UploadImageAsync(
            "uploads",
            Arg.Is<string>(name => name.EndsWith("-foto.jpg")),
            "image/jpeg",
            request.Data,
            default);

        await _publisher.Received(1).PublishAsync(
            Arg.Is<FileUploadedRequest>(r => r.FileName.EndsWith("-foto.jpg") && r.ContentType == "image/jpeg" && r.Bucket == "uploads"),
            "image-upload",
            Arg.Is<IReadOnlyDictionary<string, string>>(
                d => d["event-type"] == "file.uploaded"));
    }

    [Fact]
    public async Task Handle_WhenStorageThrows_ShouldThrowException()
    {
        // Arrange
        var handler = new ImageUploadHandler(_storage, _config, _publisher);
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF]);

        _storage.UploadImageAsync(
            "uploads",
            Arg.Is<string>(s => s.EndsWith("-foto.jpg")),
            "image/jpeg",
            request.Data,
            default)
            .ThrowsAsync(new Exception("GCS indisponível"));

        // Act
        Func<Task> act = async () => await handler.Handle(request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("GCS indisponível");
        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<FileUploadedRequest>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, string>>());
    }
}
