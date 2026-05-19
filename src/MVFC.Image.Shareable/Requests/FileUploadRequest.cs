namespace MVFC.Image.Shareable.Requests;

public record FileUploadRequest(
    string FileName,
    string ContentType,
    long Length,
    byte[] Data) :
    ICommand<Result<string>>;