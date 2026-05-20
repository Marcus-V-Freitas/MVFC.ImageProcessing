namespace MVFC.Image.Shareable.Tests.Validators;

public class FileUploadRequestValidatorTests
{
    private readonly FileUploadRequestValidator _validator = new();

    [Theory]
    [InlineData("foto.jpg", "image/jpeg")]
    [InlineData("foto.png", "image/png")]
    [InlineData("foto.webp", "image/webp")]
    [InlineData("foto.gif", "image/gif")]
    public async Task Validate_ValidAllowedTypes_ShouldPass(string fileName, string contentType)
    {
        var request = new FileUploadRequest(fileName, contentType, 1024, [0x01]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyFileName_ShouldFail(string? fileName)
    {
        var request = new FileUploadRequest(fileName!, "image/jpeg", 1024, [0x01]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.FileName));
    }

    [Fact]
    public async Task Validate_FileNameTooLong_ShouldFail()
    {
        var longFileName = new string('a', 256) + ".jpg";
        var request = new FileUploadRequest(longFileName, "image/jpeg", 1024, [0x01]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.FileName));
    }

    [Theory]
    [InlineData("foto*.jpg")]
    [InlineData("foto?.jpg")]
    [InlineData("foto/jpg")]
    [InlineData("foto\\jpg")]
    public async Task Validate_FileNameInvalidCharacters_ShouldFail(string fileName)
    {
        var request = new FileUploadRequest(fileName, "image/jpeg", 1024, [0x01]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.FileName) && e.ErrorMessage == "Nome de arquivo inválido.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyContentType_ShouldFail(string? contentType)
    {
        var request = new FileUploadRequest("foto.jpg", contentType!, 1024, [0x01]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.ContentType));
    }

    [Fact]
    public async Task Validate_ExeFile_ShouldFail()
    {
        var request = new FileUploadRequest("virus.exe", "application/octet-stream", 1024, [0x4D, 0x5A]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.ContentType));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_LengthZeroOrNegative_ShouldFail(long length)
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", length, [0x01]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Length) && e.ErrorMessage == "Arquivo vazio.");
    }

    [Fact]
    public async Task Validate_FileTooLarge_ShouldFail()
    {
        var request = new FileUploadRequest("big.jpg", "image/jpeg", 11 * 1024 * 1024, [0xFF]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Length) && e.ErrorMessage == "Arquivo excede 10MB.");
    }

    [Theory]
    [InlineData(null)]
    public async Task Validate_NullData_ShouldFail(byte[]? data)
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, data!);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Data) && e.ErrorMessage == "Dados do arquivo ausentes.");
    }

    [Fact]
    public async Task Validate_EmptyData_ShouldFail()
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, Array.Empty<byte>());
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Data) && e.ErrorMessage == "Dados do arquivo ausentes.");
    }
}
