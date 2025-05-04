using System.Text;

namespace Abx.Client.Library;

public class RealTimeTcpClient : IAbxClient
{
    private readonly ITcpClient _client;
    private INetworkStream? _stream; // Marked as nullable to address CS8618
    private bool _isRunning;
    private readonly string _server;
    private readonly int _port;

    public RealTimeTcpClient(ITcpClient client, string server, int port) // IDE0290: Primary constructor not used
    {
        _client = client;
        _server = server;
        _port = port;
        _isRunning = false;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"Connecting to {_server}:{_port}...");
            await _client.ConnectAsync(_server, _port);
            _stream = _client.GetStream();
            _isRunning = true;

            Console.WriteLine("Connected! Start typing messages (type 'exit' to quit).");

            var sendTask = Task.Run(() => SendMessagesAsync(cancellationToken), cancellationToken);
            var receiveTask = ReceiveMessagesAsync(cancellationToken);

            await Task.WhenAny(sendTask, receiveTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Stop();
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                if (_stream == null) throw new InvalidOperationException("Stream is not initialized.");
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Server disconnected.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");
            }
        }
        catch (Exception ex)
        {
            if (_isRunning) Console.WriteLine($"Receive error: {ex.Message}");
        }
    }

    private async Task SendMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                string? input = Console.ReadLine(); // Allow input to be nullable
                if (string.IsNullOrEmpty(input)) continue;

                if (input.ToLower() == "exit")
                {
                    _isRunning = false;
                    break;
                }

                if (_stream == null) throw new InvalidOperationException("Stream is not initialized.");
                byte[] data = Encoding.UTF8.GetBytes(input + Environment.NewLine);
                await _stream.WriteAsync(data, 0, data.Length, cancellationToken);
                await _stream.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send error: {ex.Message}");
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _stream?.Dispose();
        _client?.Close();
        Console.WriteLine("Disconnected.");
    }
}
