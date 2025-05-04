namespace Abx.Client.Library;

public interface ITcpClient
{
    Task ConnectAsync(string server, int port);
    INetworkStream GetStream();
    void Close();
}
