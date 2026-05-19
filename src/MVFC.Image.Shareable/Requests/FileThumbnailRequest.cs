namespace MVFC.Image.Shareable.Requests;

public sealed record FileThumbnailRequest(
    string FileName,
    string ContentType,
    long Size,
    string Bucket,
    DateTime UploadedAt)
    : FileBaseRequest(
        FileName,
        ContentType,
        Size,
        Bucket,
        UploadedAt),
    ICommand<Result>;