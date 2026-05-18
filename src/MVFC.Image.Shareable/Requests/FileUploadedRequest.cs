namespace MVFC.Image.Shareable.Requests;

public sealed class FileUploadedRequest
{
    public string FileName { get; init; } = "";

    public string ContentType { get; init; } = "";

    public long Size { get; init; }

    public string Bucket { get; init; } = "";

    public DateTime UploadedAt { get; init; }
}