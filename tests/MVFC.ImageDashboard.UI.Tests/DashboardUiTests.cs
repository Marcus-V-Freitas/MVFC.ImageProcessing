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

    [Fact]
    public async Task PostNotifyShouldReturnOk()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new DashboardUiFactory(mockMediator);

        // Act
        var response = await factory.PostNotifyAsync();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetEventsStreamShouldReturnEventStreamAndReceiveEvents()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new DashboardUiFactory(mockMediator);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await factory.GetEventsStreamAsync(cts.Token);
        
        // Dispara notificação em background para gerar o evento SSE para cobrir SseClientManager.Broadcast
        _ = Task.Run(async () =>
        {
            await Task.Delay(200, cts.Token);
            await factory.PostNotifyAsync();
        }, cts.Token);

        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);
        var eventData = string.Empty;
        var dataLine = string.Empty;

        // O primeiro evento deve chegar logo
        while (!cts.Token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cts.Token);
            if (line != null && line.StartsWith("event: ", StringComparison.Ordinal))
            {
                eventData = line;
                dataLine = await reader.ReadLineAsync(cts.Token);
                break;
            }
        }

        // Cancel the stream from the client side to test OperationCanceledException logic
        await cts.CancelAsync();
        
        // Allow the TestServer a short moment to process the aborted request,
        // throw the OperationCanceledException, and run the catch/finally blocks.
        await Task.Delay(50, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/event-stream");
        eventData.Should().Be("event: gallery-updated");
        dataLine.Should().Be("data: refresh");
    }

    [Fact]
    public async Task GetEventsStreamShouldCompleteWhenServerShutsDown()
    {
        // Arrange
        var mockMediator = Substitute.For<IMediator>();
        using var factory = new DashboardUiFactory(mockMediator);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await factory.GetEventsStreamAsync(cts.Token);
        
        // Use Task.Run to dispose the SseClientManager, simulating server shutdown
        // This will complete all channels and cause ReadAllAsync to exit normally without throwing.
        _ = Task.Run(async () =>
        {
            await Task.Delay(200, cts.Token);
            var sseManager = factory.Services.GetRequiredService<MVFC.ImageDashboard.UI.Services.SseClientManager>();
            sseManager.Dispose();
        }, cts.Token);

        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        // The channel should close and ReadLineAsync should return null, breaking the loop normally.
        var line = await reader.ReadLineAsync(cts.Token);
        
        // Brief delay to allow finally block coverage after graceful exit
        await Task.Delay(50, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/event-stream");
        line.Should().BeNull();
    }
}
