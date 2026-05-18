namespace MVFC.ImageUpload.Api.Services;

public sealed class UploadService(StorageClient storage, PublisherClient publisher)
{
    public async Task<string> UploadAsync(IFormFile file, CancellationToken ct)
    {
        var fileName = $"{Guid.NewGuid()}-{file.FileName}";
        await using var stream = file.OpenReadStream();

        await storage.UploadObjectAsync("uploads", fileName, file.ContentType, stream, cancellationToken: ct);

        var evt = new FileUploadedRequest
        {
            FileName = fileName,
            Bucket = "uploads",
            ContentType = file.ContentType,
            Size = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        await publisher.PublishAsync(new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(evt)),
            Attributes = { { "event-type", "file-uploaded" } }
        });

        return fileName;
    }
}
