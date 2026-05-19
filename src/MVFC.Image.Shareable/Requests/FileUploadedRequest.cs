namespace MVFC.Image.Shareable.Requests;

public sealed record FileUploadedRequest(
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