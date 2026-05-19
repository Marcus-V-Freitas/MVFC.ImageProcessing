namespace MVFC.Image.Shareable.Requests;

public sealed record FileConvertedRequest(
    string FileName,
    string ContentType,
    long Size,
    string Bucket,
    DateTime UploadedAt) : ICommand<Result>;