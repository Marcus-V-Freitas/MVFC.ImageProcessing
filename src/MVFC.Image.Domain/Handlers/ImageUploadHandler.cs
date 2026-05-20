namespace MVFC.Image.Domain.Handlers;

public sealed class ImageUploadHandler(
    IStorageService storage,
    AppConfigUpload appConfig,
    IPublishService publisher) : ICommandHandler<FileUploadRequest, Result<string>>
{
    public async ValueTask<Result<string>> Handle(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}-{request.FileName}";

        await storage.UploadImageAsync(appConfig.StorageConfig.UploadBucket, fileName, request.ContentType, request.Data, cancellationToken: cancellationToken);

        var evt = new FileUploadedRequest(fileName, request.ContentType, request.Length, appConfig.StorageConfig.UploadBucket, DateTime.UtcNow);
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "event-type", "file.uploaded" },
        };

        await publisher.PublishAsync(evt, appConfig.PubSubConfig.Topics["ImageUploadTopic"], attributes);

        return fileName;
    }
}
