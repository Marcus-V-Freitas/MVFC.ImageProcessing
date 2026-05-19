namespace MVFC.Image.Infra;

public sealed class StorageService(StorageClient storageClient) : IStorageService
{
    private readonly StorageClient _storageClient = storageClient;

    public async Task<MemoryStream> DownloadImageAsync(string bucketName, string objectName, CancellationToken cancellationToken)
    {
        var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(bucketName, objectName, stream, cancellationToken: cancellationToken);
        stream.Position = 0;
        return stream;
    }

    public async Task<string> UploadImageAsync(string bucketName, string objectName, string contentType, byte[] bytes, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(bytes);
        await _storageClient.UploadObjectAsync(bucketName, objectName, contentType, stream, cancellationToken: cancellationToken);
        return objectName;
    }

    public async Task DeleteImageAsync(string bucketName, string objectName, CancellationToken cancellationToken)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(bucketName, objectName, cancellationToken: cancellationToken);
        }
        catch
        {
            // Ignore
        }
    }

    public async Task<IReadOnlyList<string>> ListObjectsAsync(string bucketName, string prefix, CancellationToken cancellationToken)
    {
        var objects = new List<string>();
        await foreach (var obj in _storageClient.ListObjectsAsync(bucketName, prefix).WithCancellation(cancellationToken))
            objects.Add(obj.Name);

        return objects;
    }
}
