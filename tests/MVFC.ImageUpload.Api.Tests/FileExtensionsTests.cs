namespace MVFC.ImageUpload.Api.Tests;

public sealed class FileExtensionsTests
{
    [Fact]
    public async Task ToByteArrayAsyncShouldReturnEmptyWhenFileIsNull()
    {
        IFormFile file = null!;
        var result = await file.ToByteArrayAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToByteArrayAsyncShouldReturnEmptyWhenFileLengthIsZero()
    {
        using var stream = new MemoryStream();
        var formFile = new FormFile(stream, 0, 0, "name", "fileName.txt");
        
        var result = await formFile.ToByteArrayAsync();
        
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToByteArrayAsyncShouldReturnByteArrayWhenFileHasContent()
    {
        var bytes = Encoding.UTF8.GetBytes("test content");
        using var stream = new MemoryStream(bytes);
        var formFile = new FormFile(stream, 0, stream.Length, "name", "fileName.txt");
        
        var result = await formFile.ToByteArrayAsync();
        
        result.Should().BeEquivalentTo(bytes);
    }
}
