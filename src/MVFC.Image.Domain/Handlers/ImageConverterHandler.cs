namespace MVFC.Image.Domain.Handlers;

public sealed class ImageConverterHandler(
    IStorageService storage,
    IPublishService publisher,
    AppConfigConverter appConfig) : ICommandHandler<FileUploadedRequest, Result>
{
    public async ValueTask<Result> Handle(FileUploadedRequest request, CancellationToken cancellationToken = default)
    {
        var original = await storage.DownloadImageAsync(request.Bucket, request.FileName, cancellationToken);

        try
        {
            using var image = new MagickImage(original);
            image.Format = MagickFormat.Png;
            var bytes = image.ToByteArray();

            await storage.UploadImageAsync(request.Bucket, request.FileName, "image/png", bytes, cancellationToken: cancellationToken);

            var newEvt = new FileUploadedRequest(
                FileName: request.FileName,
                ContentType: "image/png",
                Size: bytes.Length,
                Bucket: request.Bucket,
                UploadedAt: DateTime.UtcNow
            );

            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "event-type", "file.png.converted" },
            };

            await publisher.PublishAsync(newEvt, appConfig.PubSubConfig.Topics["FileConvertTopic"], attributes);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro crítico ao tentar converter {request.FileName}. Erro: {ex.Message}");
            return Result.Fail(ex.Message);
        }
    }
}
