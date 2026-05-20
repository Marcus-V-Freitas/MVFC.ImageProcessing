namespace MVFC.ImageDashboard.UI.Tests;

public class DashboardUiTests
{
    static DashboardUiTests()
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
    public async Task DashboardUi_Endpoints_ShouldRespondCorrectly()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockMediator = Substitute.For<IMediator>();
                    mockMediator.Send<FileGalleryRequest, Result<FileGalleryResponse>>(Arg.Is<FileGalleryRequest>(r => r != null), Arg.Is<CancellationToken>(_ => true))
                        .Returns(new ValueTask<Result<FileGalleryResponse>>(Result.Ok(new FileGalleryResponse(new List<string> { "uploads/test.png" }, new List<string>(), new List<string>()))));
                    mockMediator.Send<FileDeletePublisherRequest, Result>(Arg.Is<FileDeletePublisherRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
                        .Returns(new ValueTask<Result>(Result.Ok()));
                    services.AddSingleton(mockMediator);
                });
            });

        using var client = factory.CreateClient();

        // 1. GET "/api/files"
        var getResponse = await client.GetAsync("/api/files");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var responseString = await getResponse.Content.ReadAsStringAsync();
        responseString.Should().Contain("uploads/test.png");

        // 2. POST "/api/delete/{fileName}"
        var postResponse = await client.PostAsync("/api/delete/test.png", null);
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);
    }
}
