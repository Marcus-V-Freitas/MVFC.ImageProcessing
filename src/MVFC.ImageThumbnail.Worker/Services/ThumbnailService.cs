namespace MVFC.ImageThumbnail.Worker.Services;

public sealed class ThumbnailService(StorageClient storage, PublisherClient publisher)
{
    public async Task ProcessAsync(FileUploadedRequest evt, CancellationToken ct)
    {
        using var original = new MemoryStream();

        await storage.DownloadObjectAsync(evt.Bucket, evt.FileName, original, cancellationToken: ct);
        original.Position = 0;

        using var image = new MagickImage(original);
        image.Resize(200, 200);
        image.Format = MagickFormat.Jpeg;

        using var thumbnail = new MemoryStream();
        await image.WriteAsync(thumbnail, ct);
        thumbnail.Position = 0;
        
        var thumbName = $"thumb-{evt.FileName}";
        await storage.UploadObjectAsync("thumbnails", thumbName, "image/png", thumbnail, cancellationToken: ct);

        await publisher.PublishAsync(new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(evt)),
            Attributes = { { "event-type", "thumbnail-created" } }
        });
    }
}
