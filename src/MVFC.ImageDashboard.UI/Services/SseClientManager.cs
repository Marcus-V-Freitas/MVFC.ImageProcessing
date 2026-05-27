namespace MVFC.ImageDashboard.UI.Services;

public sealed class SseClientManager : IDisposable
{
    private readonly List<Channel<string>> _clients = [];
    private readonly Lock _lock = new();

    public Channel<string> AddClient()
    {
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        lock (_lock)
        {
            _clients.Add(channel);
        }

        return channel;
    }

    public void RemoveClient(Channel<string> channel)
    {
        lock (_lock)
        {
            _clients.Remove(channel);
        }
    }

    public void Broadcast(string eventType)
    {
        lock (_lock)
        {
            foreach (var client in _clients)
            {
                client.Writer.TryWrite(eventType);
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var client in _clients)
            {
                client.Writer.TryComplete();
            }
            _clients.Clear();
        }
    }
}
