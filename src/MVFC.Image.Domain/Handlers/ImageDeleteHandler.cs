namespace MVFC.Image.Domain.Handlers;

public sealed class ImageDeleteHandler(IStorageService storage) : ICommandHandler<FileDeleteRequest, Result>
{
    public async ValueTask<Result> Handle(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        var analysisName = $"analysis-{request.FileName}.json";
        var thumbName = $"thumb-{request.FileName}";
        var tasks = new List<Task>
        {
            storage.DeleteImageAsync("uploads", request.FileName, cancellationToken: cancellationToken),
            storage.DeleteImageAsync("thumbnails", thumbName, cancellationToken: cancellationToken),
            storage.DeleteImageAsync("analysis-results", analysisName, cancellationToken: cancellationToken),
        };

        await Task.WhenAll(tasks);

        return Result.Ok();
    }
}
