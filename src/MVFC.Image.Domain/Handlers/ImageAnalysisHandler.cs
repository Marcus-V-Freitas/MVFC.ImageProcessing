namespace MVFC.Image.Domain.Handlers;

public sealed class ImageAnalysisHandler(
    IStorageService storage,
    IPublishService publisher,
    IVisionApiClient visionClient,
    AppConfigAnalysis appConfig,
    ILogger<ImageAnalysisHandler> logger) : ICommandHandler<FileConvertedRequest, Result>
{
    public async ValueTask<Result> Handle(FileConvertedRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var stream = await storage.DownloadImageAsync(appConfig.StorageConfig.UploadBucket, request.FileName, cancellationToken: cancellationToken);
            var base64Image = Convert.ToBase64String(stream.ToArray());
            var visionRequest = new VisionApiRequest(base64Image);

            var responseText = await visionClient.AnalyzeImageAsync(visionRequest, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(responseText);
            var analysisName = $"analysis-{request.FileName}.json";
            await storage.UploadImageAsync(appConfig.StorageConfig.AnalysisBucket, analysisName, "application/json", bytes, cancellationToken);

            var completedEvent = new AnalysisCompletedRequest(request.FileName);
            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "event-type", "analysis.completed" },
            };
            await publisher.PublishAsync(completedEvent, appConfig.PubSubConfig.AnalysisCompletedTopic, attributes);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorAnalyze(ex, ex.Message);
            return Result.Fail(ex.Message);
        }
    }
}