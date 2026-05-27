namespace MVFC.Image.Shareable.Tests.Extensions;

public sealed class GcsNotificationMapperTests
{
    [Fact]
    public void ToFileUploadedWithValidSizeShouldParseSizeCorrectly()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var notification = new GcsObjectNotification("test.png", "uploads", "image/png", "1024", date);

        // Act
        var request = notification.ToFileUploaded();

        // Assert
        request.FileName.Should().Be("test.png");
        request.Bucket.Should().Be("uploads");
        request.ContentType.Should().Be("image/png");
        request.Size.Should().Be(1024);
        request.UploadedAt.Should().Be(date);
    }

    [Fact]
    public void ToFileUploadedWithInvalidSizeShouldFallbackToZero()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var notification = new GcsObjectNotification("test.png", "uploads", "image/png", "not-a-number", date);

        // Act
        var request = notification.ToFileUploaded();

        // Assert
        request.Size.Should().Be(0);
    }

    [Fact]
    public void ToFileConvertedWithValidSizeShouldParseSizeCorrectly()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var notification = new GcsObjectNotification("test.png", "converted", "image/png", "2048", date);

        // Act
        var request = notification.ToFileConverted();

        // Assert
        request.FileName.Should().Be("test.png");
        request.Bucket.Should().Be("converted");
        request.ContentType.Should().Be("image/png");
        request.Size.Should().Be(2048);
        request.UploadedAt.Should().Be(date);
    }

    [Fact]
    public void ToFileConvertedWithInvalidSizeShouldFallbackToZero()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var notification = new GcsObjectNotification("test.png", "converted", "image/png", "invalid", date);

        // Act
        var request = notification.ToFileConverted();

        // Assert
        request.Size.Should().Be(0);
    }

    [Fact]
    public void ToFileThumbnailWithValidSizeShouldParseSizeCorrectly()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var notification = new GcsObjectNotification("test.png", "thumbnails", "image/png", "512", date);

        // Act
        var request = notification.ToFileThumbnail();

        // Assert
        request.FileName.Should().Be("test.png");
        request.Bucket.Should().Be("thumbnails");
        request.ContentType.Should().Be("image/png");
        request.Size.Should().Be(512);
        request.UploadedAt.Should().Be(date);
    }

    [Fact]
    public void ToFileThumbnailWithInvalidSizeShouldFallbackToZero()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var notification = new GcsObjectNotification("test.png", "thumbnails", "image/png", "", date);

        // Act
        var request = notification.ToFileThumbnail();

        // Assert
        request.Size.Should().Be(0);
    }
}
