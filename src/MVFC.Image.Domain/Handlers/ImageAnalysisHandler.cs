
namespace MVFC.Image.Domain.Handlers;

public sealed class ImageAnalysisHandler(
    IStorageService storage,
    IVisionApiClient visionClient,
    AppConfigAnalysis appConfig,
    ILogger<ImageAnalysisHandler> logger) : ICommandHandler<FileConvertedRequest, Result>
{
    public async ValueTask<Result> Handle(FileConvertedRequest request, CancellationToken cancellationToken = default)
    {
        var stream = await storage.DownloadImageAsync(appConfig.StorageConfig.UploadBucket, request.FileName, cancellationToken: cancellationToken);
        var base64Image = Convert.ToBase64String(stream.ToArray());
        var visionRequest = new VisionApiRequest(base64Image);

        try
        {
            var responseText = await visionClient.AnalyzeImageAsync(visionRequest, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(responseText);
            var analysisName = $"analysis-{request.FileName}.json";
            await storage.UploadImageAsync(appConfig.StorageConfig.AnalysisBucket, analysisName, "application/json", bytes, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorAnalyze(ex, ex.Message);
            return Result.Fail(ex.Message);
        }
    }
}