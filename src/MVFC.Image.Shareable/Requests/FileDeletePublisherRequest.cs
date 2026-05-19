namespace MVFC.Image.Shareable.Requests;

public sealed record FileDeletePublisherRequest(
    string FileName) :
    ICommand<Result>;