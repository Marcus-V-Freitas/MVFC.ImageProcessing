namespace MVFC.ImageAnalysis.Worker.Services;

public interface IVisionApiClient
{
    [Post("/analyze")]
    Task<string> AnalyzeImageAsync([Body] VisionApiRequest request, CancellationToken ct = default);
}