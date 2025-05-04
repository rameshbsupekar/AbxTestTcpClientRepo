namespace Abx.Client.Library;

public interface IAbxClient
{
    Task StartAsync(CancellationToken cancellationToken = default);
    void Stop();
}
