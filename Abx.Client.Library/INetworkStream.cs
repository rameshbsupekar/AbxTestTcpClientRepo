namespace Abx.Client.Library;

public interface INetworkStream : IDisposable
{
    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    Task FlushAsync(CancellationToken cancellationToken);
}
