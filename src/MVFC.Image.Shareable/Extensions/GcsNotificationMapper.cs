namespace MVFC.Image.Shareable.Extensions;

public static class GcsNotificationMapper
{
    public static FileUploadedRequest ToFileUploaded(this GcsObjectNotification n)
        => new(n.Name, n.ContentType, long.TryParse(n.Size, System.Globalization.CultureInfo.InvariantCulture, out var s) ? s : 0, n.Bucket, n.TimeCreated);

    public static FileConvertedRequest ToFileConverted(this GcsObjectNotification n)
        => new(n.Name, n.ContentType, long.TryParse(n.Size, System.Globalization.CultureInfo.InvariantCulture, out var s) ? s : 0, n.Bucket, n.TimeCreated);

    public static FileThumbnailRequest ToFileThumbnail(this GcsObjectNotification n)
        => new(n.Name, n.ContentType, long.TryParse(n.Size, System.Globalization.CultureInfo.InvariantCulture, out var s) ? s : 0, n.Bucket, n.TimeCreated);
}
