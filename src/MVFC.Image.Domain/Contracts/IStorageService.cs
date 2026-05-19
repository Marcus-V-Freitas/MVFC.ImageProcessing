namespace MVFC.Image.Domain.Contracts;

public interface IStorageService
{
    Task<MemoryStream> DownloadImageAsync(string bucketName, string objectName, CancellationToken cancellationToken);

    Task<string> UploadImageAsync(string bucketName, string objectName, string contentType, byte[] bytes, CancellationToken cancellationToken);

    Task DeleteImageAsync(string bucketName, string objectName, CancellationToken cancellationToken);
}