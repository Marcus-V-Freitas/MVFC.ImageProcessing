namespace MVFC.ImageDashboard.UI.Services;

public sealed class FileDeletePublisher(PublisherClient publisher)
{
    public async Task RequestDeleteAsync(string fileName)
    {
        var evt = new FileDeleteRequest(fileName);

        await publisher.PublishAsync(new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(evt)),
            Attributes = { { "event-type", "file-delete-requested" } },
        });
    }
}
