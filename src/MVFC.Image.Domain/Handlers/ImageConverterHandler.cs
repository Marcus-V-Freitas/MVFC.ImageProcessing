namespace MVFC.Image.Domain.Handlers;

public sealed class ImageConverterHandler(
    IStorageService storage,
    AppConfigConverter appConfig,
    ILogger<ImageConverterHandler> logger) : ICommandHandler<FileUploadedRequest, Result>
{
    public async ValueTask<Result> Handle(FileUploadedRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var original = await storage.DownloadImageAsync(request.Bucket, request.FileName, cancellationToken);

            using var image = new MagickImage(original);
            image.Format = MagickFormat.Png;
            var bytes = image.ToByteArray();

            await storage.UploadImageAsync(appConfig.StorageConfig.ConvertedBucket, request.FileName, "image/png", bytes, cancellationToken: cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorConvert(ex, request.FileName);
            return Result.Fail(ex.Message);
        }
    }
}
