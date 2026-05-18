namespace MVFC.ImageConverter.Worker.Services;

public sealed class ImageNormalizerService(StorageClient storage, PublisherClient publisher)
{
    public async Task ProcessAsync(FileUploadedRequest evt, CancellationToken ct)
    {
        Console.WriteLine($"Normalizando: {evt.FileName}");

        var original = new MemoryStream();
        await storage.DownloadObjectAsync(evt.Bucket, evt.FileName, original, cancellationToken: ct);
        original.Position = 0;

        try 
        {
            using var image = new MagickImage(original);
            image.Format = MagickFormat.Png;

            using var converted = new MemoryStream();
            await image.WriteAsync(converted, ct);
            converted.Position = 0;

            await storage.UploadObjectAsync(evt.Bucket, evt.FileName, "image/png", converted, cancellationToken: ct);

            var newEvt = new FileUploadedRequest
            {
                FileName = evt.FileName,
                Bucket = evt.Bucket,
                ContentType = "image/png",
                Size = converted.Length,
                UploadedAt = DateTime.UtcNow
            };

            await publisher.PublishAsync(new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(newEvt)),
                Attributes = { { "event-type", "file-normalized" } }
            });

            Console.WriteLine($"Arquivo normalizado para PNG em-lugar: {evt.FileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro crítico ao tentar normalizar {evt.FileName}. Erro: {ex.Message}");
        }
    }
}
