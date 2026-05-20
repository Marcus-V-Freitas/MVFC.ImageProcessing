namespace MVFC.Image.Domain.Contracts;

public interface IStorageService
{
    Task<MemoryStream> DownloadImageAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);

    Task<string> UploadImageAsync(string bucketName, string objectName, string contentType, byte[] bytes, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListObjectsAsync(string bucketName, string prefix, CancellationToken cancellationToken = default);
}