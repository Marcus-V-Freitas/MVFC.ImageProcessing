namespace MVFC.Image.Shareable.Tests.Validators;

public sealed class FileUploadRequestValidatorTests
{
    private readonly FileUploadRequestValidator _validator = new();

    [Theory]
    [InlineData("foto.jpg", "image/jpeg")]
    [InlineData("foto.png", "image/png")]
    [InlineData("foto.webp", "image/webp")]
    [InlineData("foto.gif", "image/gif")]
    public async Task ValidateValidAllowedTypesShouldPass(string fileName, string contentType)
    {
        var request = new FileUploadRequest(fileName, contentType, 1024, [0x01]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateEmptyFileNameShouldFail(string? fileName)
    {
        var request = new FileUploadRequest(fileName!, "image/jpeg", 1024, [0x01]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.FileName));
    }

    [Fact]
    public async Task ValidateFileNameTooLongShouldFail()
    {
        var longFileName = new string('a', 256) + ".jpg";
        var request = new FileUploadRequest(longFileName, "image/jpeg", 1024, [0x01]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.FileName));
    }

    [Theory]
    [InlineData("foto*.jpg")]
    [InlineData("foto?.jpg")]
    [InlineData("foto/jpg")]
    [InlineData("foto\\jpg")]
    public async Task ValidateFileNameInvalidCharactersShouldFail(string fileName)
    {
        var request = new FileUploadRequest(fileName, "image/jpeg", 1024, [0x01]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.FileName) && e.ErrorMessage == "Nome de arquivo inválido, contém caracteres não permitidos.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateEmptyContentTypeShouldFail(string? contentType)
    {
        var request = new FileUploadRequest("foto.jpg", contentType!, 1024, [0x01]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.ContentType));
    }

    [Fact]
    public async Task ValidateExeFileShouldFail()
    {
        var request = new FileUploadRequest("virus.exe", "application/octet-stream", 1024, [0x4D, 0x5A]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.ContentType));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateLengthZeroOrNegativeShouldFail(long length)
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", length, [0x01]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Length) && e.ErrorMessage == "Arquivo vazio.");
    }

    [Fact]
    public async Task ValidateFileTooLargeShouldFail()
    {
        var request = new FileUploadRequest("big.jpg", "image/jpeg", 11 * 1024 * 1024, [0xFF]);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Length) && e.ErrorMessage == "Arquivo excede 10MB.");
    }

    [Theory]
    [InlineData(null)]
    public async Task ValidateNullDataShouldFail(byte[]? data)
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, data!);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Data) && e.ErrorMessage == "Dados do arquivo ausentes.");
    }

    [Fact]
    public async Task ValidateEmptyDataShouldFail()
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, []);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Data) && e.ErrorMessage == "Dados do arquivo ausentes.");
    }
}