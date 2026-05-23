namespace MVFC.ImageDashboard.UI.Tests;

public sealed class DashboardUiTests
{
    [Fact]
    public async Task GetFilesShouldReturnOk()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileGalleryRequest, Result<FileGalleryResponse>>(Arg.Is<FileGalleryRequest>(r => r != null), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result<FileGalleryResponse>>(Result.Ok(new FileGalleryResponse(["uploads/test.png"], [], []))));
        using var factory = new DashboardUiFactory(mockMediator);

        // Act
        var getResponse = await factory.GetFilesAsync();

        // Assert
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var responseString = await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseString.Should().Contain("uploads/test.png");
    }

    [Fact]
    public async Task PostDeleteShouldReturnAccepted()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        mockMediator.Send<FileDeletePublisherRequest, Result>(Arg.Is<FileDeletePublisherRequest>(r => r.FileName == "test.png"), Arg.Is<CancellationToken>(_ => true))
            .Returns(new ValueTask<Result>(Result.Ok()));
        using var factory = new DashboardUiFactory(mockMediator);

        // Act
        var postResponse = await factory.PostDeleteAsync("test.png");

        // Assert
        postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);
    }
}
