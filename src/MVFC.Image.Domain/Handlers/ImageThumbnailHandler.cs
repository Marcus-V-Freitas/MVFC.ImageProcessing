namespace MVFC.Image.Domain.Handlers;

public sealed class ImageThumbnailHandler(
    IStorageService storage,
    IPublishService publisher,
    AppConfigThumbnail appConfig) : ICommandHandler<FileThumbnailRequest, Result>
{
    public async ValueTask<Result> Handle(FileThumbnailRequest request, CancellationToken cancellationToken = default)
    {
        var original = await storage.DownloadImageAsync(request.Bucket, request.FileName, cancellationToken: cancellationToken);
        original.Position = 0;

        using var image = new MagickImage(original);
        image.Resize(200, 200);
        image.Format = MagickFormat.Jpeg;
        var bytes = image.ToByteArray();

        var thumbName = $"thumb-{request.FileName}";
        await storage.UploadImageAsync("thumbnails", thumbName, "image/png", bytes, cancellationToken: cancellationToken);

        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "event-type", "thumbnail-created" }
        };

        await publisher.PublishAsync(request, appConfig.PubSubConfig.Topics["ThumbnailCreatedTopic"], attributes);

        return Result.Ok();
    }
}
