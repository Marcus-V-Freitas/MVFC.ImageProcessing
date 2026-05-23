namespace MVFC.ImageUpload.Api.Tests;

public sealed class UploadApiTests
{
    [Fact]
    public async Task GetRootShouldReturnOk()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new UploadApiFactory(mockMediator);

        // Act
        var response = await factory.GetRootAsync();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("mvfc-image-upload-api ok");
    }

    [Fact]
    public async Task PostUploadShouldReturnAcceptedWhenFileIsValid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileUploadRequest, Result<string>>(Arg.Is<FileUploadRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result<string>>(Result.Ok("uploads/guid-test.png")));
        using var factory = new UploadApiFactory(mockMediator);

        using var multipartContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        multipartContent.Add(fileContent, "file", "test.png");

        // Act
        var response = await factory.PostUploadAsync(multipartContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task PostUploadShouldReturnBadRequestWhenFileIsInvalid()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new UploadApiFactory(mockMediator);

        using var invalidMultipart = new MultipartFormDataContent();
        var invalidFileContent = new ByteArrayContent([0x89]);
        invalidFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        invalidMultipart.Add(invalidFileContent, "file", "invalid.txt");

        // Act
        var response = await factory.PostUploadAsync(invalidMultipart);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostUploadShouldReturnUnprocessableEntityWhenMediatorFails()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileUploadRequest, Result<string>>(Arg.Is<FileUploadRequest>(r => r.FileName == "fail.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result<string>>(Result.Fail<string>("Error occurred")));
        using var factory = new UploadApiFactory(mockMediator);

        using var failMultipart = new MultipartFormDataContent();
        var failFileContent = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        failFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        failMultipart.Add(failFileContent, "file", "fail.png");

        // Act
        var response = await factory.PostUploadAsync(failMultipart);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
