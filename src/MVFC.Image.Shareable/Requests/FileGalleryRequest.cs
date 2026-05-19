namespace MVFC.Image.Shareable.Requests;

public sealed record FileGalleryRequest() :
    ICommand<Result<FileGalleryResponse>>;