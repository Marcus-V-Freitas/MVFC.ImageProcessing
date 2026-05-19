namespace MVFC.Image.Shareable.Response;

public sealed record FileGalleryResponse(
    IReadOnlyList<string> Uploads,
    IReadOnlyList<string> Thumbnails,
    IReadOnlyList<string> Analyses);