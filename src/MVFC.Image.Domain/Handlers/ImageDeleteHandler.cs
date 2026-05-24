namespace MVFC.Image.Domain.Handlers;

public sealed class ImageDeleteHandler(
    IStorageService storage,
    StorageConfig storageConfig,
    ILogger<ImageDeleteHandler> logger) : ICommandHandler<FileDeleteRequest, Result>
{
    public async ValueTask<Result> Handle(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysisName = $"analysis-{request.FileName}.json";
            var baseName = System.IO.Path.GetFileNameWithoutExtension(request.FileName);
            var thumbName = $"thumb-{baseName}.png";
            var tasks = new List<Task>
            {
                storage.DeleteImageAsync(storageConfig.UploadBucket, request.FileName, cancellationToken: cancellationToken),
                storage.DeleteImageAsync(storageConfig.ThumbnailBucket, thumbName, cancellationToken: cancellationToken),
                storage.DeleteImageAsync(storageConfig.AnalysisBucket, analysisName, cancellationToken: cancellationToken),
            };

            await Task.WhenAll(tasks);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorDelete(ex, request.FileName);
            return Result.Fail(ex.Message);
        }
    }
}
