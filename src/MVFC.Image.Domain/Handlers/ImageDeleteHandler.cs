namespace MVFC.Image.Domain.Handlers;

public sealed class ImageDeleteHandler(IStorageService storage, StorageConfig storageConfig) : ICommandHandler<FileDeleteRequest, Result>
{
    public async ValueTask<Result> Handle(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        var analysisName = $"analysis-{request.FileName}.json";
        var thumbName = $"thumb-{request.FileName}";
        var tasks = new List<Task>
        {
            storage.DeleteImageAsync(storageConfig.UploadBucket, request.FileName, cancellationToken: cancellationToken),
            storage.DeleteImageAsync(storageConfig.ThumbnailBucket, thumbName, cancellationToken: cancellationToken),
            storage.DeleteImageAsync(storageConfig.AnalysisBucket, analysisName, cancellationToken: cancellationToken),
        };

        await Task.WhenAll(tasks);

        return Result.Ok();
    }
}
