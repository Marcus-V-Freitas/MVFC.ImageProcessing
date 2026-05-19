namespace MVFC.Image.Domain.Handlers;

public sealed class ImageDeletePublisherHandler(
    IPublishService publisher,
    AppConfigDashboard appConfig) : ICommandHandler<FileDeletePublisherRequest, Result>
{
    public async ValueTask<Result> Handle(FileDeletePublisherRequest request, CancellationToken cancellationToken = default)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "event-type", "file-delete-requested" },
        };

        await publisher.PublishAsync(request, appConfig.PubSubConfig.Topics["FileDeleteRequestedTopic"], attributes);

        return Result.Ok();
    }
}