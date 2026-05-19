namespace MVFC.Image.Shareable.Requests.Base;

public abstract record FileBaseRequest(
    string FileName,
    string ContentType,
    long Size,
    string Bucket,
    DateTime UploadedAt);