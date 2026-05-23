namespace MVFC.Image.Shareable.Tests.Configs;

public sealed class ConfigurationExtensionsTests
{
    private sealed record TestConfig(string Name);

    [Fact]
    public void GetRequiredConfigShouldReturnConfigWhenConfigurationIsValid()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Name", "ValidName"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var result = configuration.GetRequiredConfig<TestConfig>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ValidName", result.Name);
    }

    [Fact]
    public void GetRequiredConfigShouldThrowInvalidOperationExceptionWhenConfigurationIsMissing()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var act = () => configuration.GetRequiredConfig<TestConfig>();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("TestConfig", exception.Message);
    }

    [Fact]
    public void GetRequiredConfigWithSectionShouldReturnConfigWhenSectionIsValid()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"MySection:Name", "SectionValidName"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var result = configuration.GetRequiredConfig<TestConfig>("MySection");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("SectionValidName");
    }

    [Fact]
    public void GetRequiredConfigWithSectionShouldThrowInvalidOperationExceptionWhenSectionIsMissing()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var act = () => configuration.GetRequiredConfig<TestConfig>("MissingSection");

        // Assert
        var exception = act.Should().Throw<InvalidOperationException>()
                           .Which.Message.Should().Contain("MissingSection");
    }
}
