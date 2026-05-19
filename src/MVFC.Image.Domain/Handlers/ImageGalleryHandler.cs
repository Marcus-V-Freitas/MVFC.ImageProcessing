namespace MVFC.Image.Domain.Handlers;

public sealed class ImageGalleryHandler(IStorageService storage) : ICommandHandler<FileGalleryRequest, Result<FileGalleryResponse>>
{
    public async ValueTask<Result<FileGalleryResponse>> Handle(FileGalleryRequest request, CancellationToken cancellationToken = default)
    {
        var uploads = await storage.ListObjectsAsync("uploads", "", cancellationToken);
        var thumbnails = await storage.ListObjectsAsync("thumbnails", "", cancellationToken);
        var analyses = await storage.ListObjectsAsync("analysis-results", "", cancellationToken);

        return new FileGalleryResponse(uploads, thumbnails, analyses);
    }
}