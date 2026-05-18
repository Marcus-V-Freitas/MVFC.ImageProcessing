namespace MVFC.ImageAnalysis.Worker.Services;

public sealed class ImageAnalysisService(StorageClient storage, IVisionApiClient visionClient)
{
    public async Task ProcessAsync(FileUploadedRequest evt, CancellationToken ct)
    {
        Console.WriteLine($"Recebido evento para análise: {evt.FileName}");

        using var stream = new MemoryStream();
        await storage.DownloadObjectAsync("uploads", evt.FileName, stream, cancellationToken: ct);

        var base64Image = Convert.ToBase64String(stream.ToArray());
        var visionRequest = new VisionApiRequest(base64Image);

        try
        {
            var responseText = await visionClient.AnalyzeImageAsync(visionRequest, ct);
            Console.WriteLine($"Resposta do Vision API: {responseText}");

            using var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(responseText));

            var analysisName = $"analysis-{evt.FileName}.json";
            await storage.UploadObjectAsync("analysis-results", analysisName, "application/json", resultStream, cancellationToken: ct);

            Console.WriteLine($"Análise salva: {analysisName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao analisar: {ex.Message}");
        }
    }
}
