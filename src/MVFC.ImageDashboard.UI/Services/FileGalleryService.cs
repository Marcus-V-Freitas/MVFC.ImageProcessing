namespace MVFC.ImageDashboard.UI.Services;

public sealed class FileGalleryService(StorageClient storage)
{
    public async Task<object> ListFilesAsync()
    {
        var uploads = new List<string>();
        var thumbnails = new List<string>();
        var analyses = new List<string>();

        await foreach (var obj in storage.ListObjectsAsync("uploads", ""))
            uploads.Add(obj.Name);
        
        await foreach (var obj in storage.ListObjectsAsync("thumbnails", ""))
            thumbnails.Add(obj.Name);

        await foreach (var obj in storage.ListObjectsAsync("analysis-results", ""))
            analyses.Add(obj.Name);

        return new { uploads, thumbnails, analyses };
    }
}