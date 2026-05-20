namespace MVFC.ImageUpload.Api.Tests;

public class UploadApiTests
{
    static UploadApiTests()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("STORAGE_EMULATOR_HOST", "http://localhost:8080");
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "http://localhost:8085");
        Environment.SetEnvironmentVariable("VisualApiUrl", "http://localhost:5000");
        Environment.SetEnvironmentVariable("StorageConfig:UploadBucket", "uploads");
        Environment.SetEnvironmentVariable("StorageConfig:ThumbnailBucket", "thumbnails");
        Environment.SetEnvironmentVariable("StorageConfig:AnalysisBucket", "analysis-results");
        Environment.SetEnvironmentVariable("PubSubConfig:ProjectId", "proj-test");
        Environment.SetEnvironmentVariable("PubSubConfig:Topics:FileUploadedTopic", "file-uploaded");
        Environment.SetEnvironmentVariable("PubSubConfig:Topics:FileDeletePublisherTopic", "delete-requested");
        Environment.SetEnvironmentVariable("PubSubConfig:Topics:ImageUploadTopic", "image-upload");
        Environment.SetEnvironmentVariable("PubSubConfig:Topics:ThumbnailCreatedTopic", "thumbnail-created");
    }

    [Fact]
    public async Task UploadApi_Endpoints_ShouldRespondCorrectly()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockMediator = Substitute.For<IMediator>();
                    mockMediator.Send<FileUploadRequest, Result<string>>(Arg.Is<FileUploadRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
                        .Returns(new ValueTask<Result<string>>(Result.Ok("uploads/guid-test.png")));
                    mockMediator.Send<FileUploadRequest, Result<string>>(Arg.Is<FileUploadRequest>(r => r.FileName == "fail.png"), Arg.Is<CancellationToken>(_ => true))
                        .Returns(new ValueTask<Result<string>>(Result.Fail<string>("Error occurred")));
                    services.AddSingleton(mockMediator);
                });
            });

        using var client = factory.CreateClient();

        // 1. GET "/"
        var getResponse = await client.GetAsync("/");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        (await getResponse.Content.ReadAsStringAsync()).Should().Be("mvfc-image-upload-api ok");

        // 2. POST "/upload" - Success Path
        using var multipartContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]); // PNG signature
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        multipartContent.Add(fileContent, "file", "test.png");
        var response = await client.PostAsync("/upload", multipartContent);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);

        // 3. POST "/upload" - Validation Failure Path (Invalid ContentType/Extension)
        using var invalidMultipart = new MultipartFormDataContent();
        var invalidFileContent = new ByteArrayContent([0x89]);
        invalidFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        invalidMultipart.Add(invalidFileContent, "file", "invalid.txt");
        var invalidResponse = await client.PostAsync("/upload", invalidMultipart);
        invalidResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // 4. POST "/upload" - Mediator Failure Path
        using var failMultipart = new MultipartFormDataContent();
        var failFileContent = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        failFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        failMultipart.Add(failFileContent, "file", "fail.png");
        var failResponse = await client.PostAsync("/upload", failMultipart);
        failResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
