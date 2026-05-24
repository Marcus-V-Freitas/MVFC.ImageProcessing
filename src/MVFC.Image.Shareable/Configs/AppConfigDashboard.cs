namespace MVFC.Image.Shareable.Configs;

public sealed record AppConfigDashboard(
    [property: Required] PubSubConfig PubSubConfig);