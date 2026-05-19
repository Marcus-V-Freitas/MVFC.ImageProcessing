namespace MVFC.Image.Domain.Handlers;

public sealed class ImageAnalysisHandler(IStorageService storage, IVisionApiClient visionClient) : ICommandHandler<FileConvertedRequest, Result>
{
    public async ValueTask<Result> Handle(FileConvertedRequest request, CancellationToken cancellationToken = default)
    {
        var stream = await storage.DownloadImageAsync("uploads", request.FileName, cancellationToken: cancellationToken);
        var base64Image = Convert.ToBase64String(stream.ToArray());
        var visionRequest = new VisionApiRequest(base64Image);

        try
        {
            var responseText = await visionClient.AnalyzeImageAsync(visionRequest, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(responseText);
            var analysisName = $"analysis-{request.FileName}.json";
            await storage.UploadImageAsync("analysis-results", analysisName, "application/json", bytes, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao analisar: {ex.Message}");
            return Result.Fail(ex.Message);
        }
    }
}