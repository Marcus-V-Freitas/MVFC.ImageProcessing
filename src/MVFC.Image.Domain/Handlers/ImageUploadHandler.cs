namespace MVFC.Image.Domain.Handlers;

public sealed class ImageUploadHandler(
    IStorageService storage,
    AppConfigUpload appConfig) : ICommandHandler<FileUploadRequest, Result<string>>
{
    public async ValueTask<Result<string>> Handle(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}-{request.FileName}";

        await storage.UploadImageAsync(appConfig.StorageConfig.UploadBucket, fileName, request.ContentType, request.Data, cancellationToken: cancellationToken);

        return fileName;
    }
}
