namespace MVFC.Image.Infra.Tests;

public sealed class StorageServiceTests
{
    [Fact]
    public async Task DownloadImageAsyncShouldReturnStream()
    {

        var storageClient = Substitute.For<StorageClient>();
        var service = new StorageService(storageClient);

        var result = await service.DownloadImageAsync("bucket", "object", CancellationToken.None);

        result.Should().NotBeNull();
        result.Length.Should().Be(0);
    }

    [Fact]
    public async Task UploadImageAsyncShouldReturnObjectName()
    {
        var storageClient = Substitute.For<StorageClient>();
        var service = new StorageService(storageClient);

        var bytes = System.Text.Encoding.UTF8.GetBytes("test");
        var result = await service.UploadImageAsync("bucket", "object", "image/png", bytes, CancellationToken.None);

        result.Should().Be("object");
    }

    [Fact]
    public async Task DeleteImageAsyncShouldSwallow404Exception()
    {
        var storageClient = Substitute.For<StorageClient>();
        storageClient.DeleteObjectAsync("bucket", "object", null, CancellationToken.None)
                     .Returns(Task.FromException(new Google.GoogleApiException("Google", "Not found") 
                     { 
                         HttpStatusCode = System.Net.HttpStatusCode.NotFound 
                     }));

        var service = new StorageService(storageClient);

        await service.Invoking(s => s.DeleteImageAsync("bucket", "object", CancellationToken.None))
                     .Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteImageAsyncShouldNotThrowIfNoException()
    {
        var storageClient = Substitute.For<StorageClient>();
        var service = new StorageService(storageClient);

        await service.Invoking(s => s.DeleteImageAsync("bucket", "object", CancellationToken.None))
                     .Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListObjectsAsyncShouldReturnObjects()
    {
        var storageClient = Substitute.For<StorageClient>();        
        var mockEnum = Substitute.For<PagedAsyncEnumerable<StorageObjects, StorageObject>>();        
        var list = new List<StorageObject> 
        { 
            new() { Name = "file1" },
        };

        mockEnum.GetAsyncEnumerator(CancellationToken.None)
                .Returns(list.ToAsyncEnumerable().GetAsyncEnumerator(TestContext.Current.CancellationToken));

        storageClient.ListObjectsAsync("bucket", "prefix", null)
                     .Returns(mockEnum);

        var service = new StorageService(storageClient);

        var result = await service.ListObjectsAsync("bucket", "prefix", CancellationToken.None);
        
        result.Should().ContainSingle().Which.Should().Be("file1");
    }
}
