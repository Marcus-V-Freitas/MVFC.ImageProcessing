namespace MVFC.Image.Shareable.Requests;

public sealed record GcsObjectNotification(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("bucket")] string Bucket,
    [property: JsonPropertyName("contentType")] string ContentType,
    [property: JsonPropertyName("size")] string Size,
    [property: JsonPropertyName("timeCreated")] DateTime TimeCreated);
