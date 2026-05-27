namespace MVFC.Image.Shareable.Requests;

public sealed record AnalysisCompletedRequest(string FileName) : ICommand;
