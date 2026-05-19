namespace MVFC.ImageUpload.Api.Extensions;

public static class FileExtensions
{
    internal async static Task<byte[]> ToByteArrayAsync(this IFormFile file)
    {
        if (file is null || file.Length == 0)
            return [];

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }
}