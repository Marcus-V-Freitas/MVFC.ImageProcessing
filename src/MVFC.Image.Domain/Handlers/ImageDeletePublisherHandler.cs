namespace MVFC.Image.Domain.Handlers;

public sealed class ImageDeletePublisherHandler(
    IPublishService publisher,
    AppConfigDashboard appConfig,
    ILogger<ImageDeletePublisherHandler> logger) : ICommandHandler<FileDeletePublisherRequest, Result>
{
    public async ValueTask<Result> Handle(FileDeletePublisherRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "event-type", "file-delete-requested" },
            };

            await publisher.PublishAsync(request, appConfig.PubSubConfig.FileDeleteRequestedTopic, attributes);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorDeletePublish(ex, request.FileName);
            return Result.Fail(ex.Message);
        }
    }
}