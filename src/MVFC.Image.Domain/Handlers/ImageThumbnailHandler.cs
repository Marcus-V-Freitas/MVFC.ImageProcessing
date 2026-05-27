namespace MVFC.Image.Domain.Handlers;

public sealed class ImageThumbnailHandler(
    IStorageService storage,
    AppConfigThumbnail appConfig,
    ILogger<ImageThumbnailHandler> logger) : ICommandHandler<FileThumbnailRequest, Result>
{
    public async ValueTask<Result> Handle(FileThumbnailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var original = await storage.DownloadImageAsync(request.Bucket, request.FileName, cancellationToken: cancellationToken);
            original.Position = 0;

            using var image = new MagickImage(original);
            image.Resize(new MagickGeometry(200, 200) { IgnoreAspectRatio = false });
            image.Format = MagickFormat.Png;
            var bytes = image.ToByteArray();

            var baseName = Path.GetFileNameWithoutExtension(request.FileName);
            var thumbName = $"thumb-{baseName}.png";
            await storage.UploadImageAsync(appConfig.StorageConfig.ThumbnailBucket, thumbName, "image/png", bytes, cancellationToken: cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorThumbnail(ex, request.FileName);
            return Result.Fail(ex.Message);
        }
    }
}
