namespace MVFC.ImageAnalysis.Worker.Tests;

public class AnalysisWorkerTests
{
    static AnalysisWorkerTests()
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
    public async Task AnalysisWorker_Endpoints_ShouldRespondCorrectly()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockMediator = Substitute.For<IMediator>();
                    mockMediator.Send<FileConvertedRequest, Result>(Arg.Is<FileConvertedRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
                        .Returns(new ValueTask<Result>(Result.Ok()));
                    mockMediator.Send<FileConvertedRequest, Result>(Arg.Is<FileConvertedRequest>(r => r.FileName == "fail.png"), Arg.Is<CancellationToken>(_ => true))
                        .Returns(new ValueTask<Result>(Result.Fail("Error occurred")));
                    services.AddSingleton(mockMediator);
                });
            });

        using var client = factory.CreateClient();

        // 1. GET "/"
        var getResponse = await client.GetAsync("/");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        (await getResponse.Content.ReadAsStringAsync()).Should().Be("mvfc-image-analysis-worker ok");

        // 2. POST "/pubsub/push" - Success Path
        var payload = new FileConvertedRequest("test.png", "image/png", 100, "uploads", DateTime.UtcNow);
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        var pubsubRequest = new PubSubRequest(
            new PubSubMessageRequest(base64Data, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );
        var content = new StringContent(JsonSerializer.Serialize(pubsubRequest), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/pubsub/push", content);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 3. POST "/pubsub/push" - Empty Payload Path
        var emptyRequest = new PubSubRequest(
            new PubSubMessageRequest("", "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );
        var emptyContent = new StringContent(JsonSerializer.Serialize(emptyRequest), Encoding.UTF8, "application/json");
        var emptyResponse = await client.PostAsync("/pubsub/push", emptyContent);
        emptyResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // 4. POST "/pubsub/push" - Deserialization Failure Path (returns null)
        var nullBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("null"));
        var invalidRequest = new PubSubRequest(
            new PubSubMessageRequest(nullBase64, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );
        var invalidContent = new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json");
        var invalidResponse = await client.PostAsync("/pubsub/push", invalidContent);
        invalidResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // 5. POST "/pubsub/push" - Mediator Failure Path
        var failPayload = new FileConvertedRequest("fail.png", "image/png", 100, "uploads", DateTime.UtcNow);
        var failBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(failPayload)));
        var failRequest = new PubSubRequest(
            new PubSubMessageRequest(failBase64, "msg-123", "2026-05-19T20:00:00Z", new Dictionary<string, string>()),
            "sub-1"
        );
        var failContent = new StringContent(JsonSerializer.Serialize(failRequest), Encoding.UTF8, "application/json");
        var failResponse = await client.PostAsync("/pubsub/push", failContent);
        failResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
