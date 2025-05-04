using System.Net.Sockets;

namespace Abx.Client.Library;

public class NetworkStreamWrapper : INetworkStream
{
    private readonly NetworkStream _networkStream;

    public NetworkStreamWrapper(NetworkStream networkStream)
    {
        _networkStream = networkStream;
    }

    public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await _networkStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await _networkStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _networkStream.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        _networkStream.Dispose();
    }
}

