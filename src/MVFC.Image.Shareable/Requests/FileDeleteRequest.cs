namespace MVFC.Image.Shareable.Requests;

public sealed record FileDeleteRequest(string FileName) : ICommand<Result>;