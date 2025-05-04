// See https://aka.ms/new-console-template for more information
namespace Abx.Client.Library;

public interface ITcpSettings
{
    int Port { get; }
    string Host { get; }
}

public class TcpSettings : ITcpSettings
{
    public int Port { get; }

    public string Host { get; }

    public TcpSettings(int port, string host)
    {
        Port = port;
        Host = host;
    }
}