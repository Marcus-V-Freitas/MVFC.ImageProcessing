namespace MVFC.ImageDelete.Worker.Services;

public sealed class DeleteService(StorageClient storage)
{
    public async Task DeleteAsync(FileDeleteRequest evt, CancellationToken ct)
    {
        Console.WriteLine($"Delete requested for: {evt.FileName}");

        var tasks = new List<Task>();

        try 
        { 
            tasks.Add(storage.DeleteObjectAsync("uploads", evt.FileName, cancellationToken: ct)); 
        } 
        catch 
        { 
            //ignore
        }
        
        var thumbName = $"thumb-{evt.FileName}";
        try 
        { 
            tasks.Add(storage.DeleteObjectAsync("thumbnails", thumbName, cancellationToken: ct)); 
        } 
        catch 
        { 
            //ignore
        }
        
        var analysisName = $"analysis-{evt.FileName}.json";
        try 
        { 
            tasks.Add(storage.DeleteObjectAsync("analysis-results", analysisName, cancellationToken: ct)); 
        } 
        catch 
        { 
            //ignore
        }

        await Task.WhenAll(tasks);

        Console.WriteLine($"Files for {evt.FileName} deleted successfully.");
    }
}
