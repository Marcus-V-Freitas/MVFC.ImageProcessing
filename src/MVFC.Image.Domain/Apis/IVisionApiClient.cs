namespace MVFC.Image.Domain.Apis;

public interface IVisionApiClient
{
    [Post("/analyze")]
    Task<string> AnalyzeImageAsync([Body] VisionApiRequest request, CancellationToken ct = default);
}