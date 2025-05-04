using System.Net.Sockets;

namespace Abx.Client.Library;

public class TcpClientWrapper : ITcpClient
{
    private readonly TcpClient _tcpClient = new();

    public async Task ConnectAsync(string server, int port)
    {
        await _tcpClient.ConnectAsync(server, port);
    }

    public byte[] CreatePayload(byte callType, byte resendSeq = 0)
    {
        // Validate callType
        if (callType != 1 && callType != 2)
            throw new ArgumentException("Invalid callType. Use 1 for 'Stream All Packets' or 2 for 'Resend Packet'.");

        // Create the payload
        return new byte[] { callType, resendSeq };
    }

    public async Task SendRequestAsync(ITcpClient tcpClient, byte callType, byte resendSeq = 0, CancellationToken cancellationToken = default)
    {
        // Create the payload
        var payload = CreatePayload(callType, resendSeq);

        // Get the network stream
        var stream = tcpClient.GetStream();

        // Send the payload
        await stream.WriteAsync(payload, 0, payload.Length, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public async Task StreamAllPacketsAsync(CancellationToken cancellationToken = default)
    {
        var payload = CreatePayload(callType: 1);
        var stream = GetStream();

        await stream.WriteAsync(payload, 0, payload.Length, cancellationToken);
        await stream.FlushAsync(cancellationToken);

        byte[] buffer = new byte[1024];
        var receivedSequences = new HashSet<int>();
        int lastSequence = -1;

        try
        {
            Console.WriteLine("Receiving packets from the server...");

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead == 0)
                {
                    Console.WriteLine("Server has closed the connection.");
                    break;
                }

                var packets = ParseResponsePayload(buffer, bytesRead);

                foreach (var packet in packets)
                {
                    Console.WriteLine($"Received Packet: {packet.Symbol}, {packet.BuySellIndicator}, {packet.Quantity}, {packet.Price}, {packet.Sequence}");
                    receivedSequences.Add(packet.Sequence);
                    lastSequence = Math.Max(lastSequence, packet.Sequence);
                }
            }

            // Identify missing sequences
            var missingSequences = Enumerable.Range(1, lastSequence - 1).Except(receivedSequences).ToList();

            Console.WriteLine($"Missing Sequences: {string.Join(", ", missingSequences)}");

            // Request missing packets
            foreach (var seq in missingSequences)
            {
                await ResendPacketAsync(seq, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while streaming packets: {ex.Message}");
        }
        finally
        {
            Close();
        }
    }

    public INetworkStream GetStream()
    {
        return new NetworkStreamWrapper(_tcpClient.GetStream());
    }

    public void Close()
    {
        _tcpClient.Close();
    }

    private static List<OrderPacket> ParseResponsePayload(byte[] buffer, int bytesRead)
    {
        const int packetSize = 17; // 4 + 1 + 4 + 4 + 4 (Symbol + Buy/Sell + Quantity + Price + Sequence)
        var packets = new List<OrderPacket>();

        for (int i = 0; i < bytesRead; i += packetSize)
        {
            if (i + packetSize > bytesRead) break; // Ensure we don't read beyond the buffer

            var symbol = System.Text.Encoding.ASCII.GetString(buffer, i, 4);
            var buySellIndicator = (char)buffer[i + 4];
            var quantity = BitConverter.ToInt32(buffer.Skip(i + 5).Take(4).Reverse().ToArray(), 0); // Big Endian
            var price = BitConverter.ToInt32(buffer.Skip(i + 9).Take(4).Reverse().ToArray(), 0); // Big Endian
            var sequence = BitConverter.ToInt32(buffer.Skip(i + 13).Take(4).Reverse().ToArray(), 0); // Big Endian

            packets.Add(new OrderPacket(symbol, buySellIndicator, quantity, price, sequence));
        }

        return packets;
    }

    public async Task ResendPacketAsync(int sequence, CancellationToken cancellationToken = default)
    {
        var payload = CreatePayload(callType: 2, resendSeq: (byte)sequence);
        var stream = GetStream();

        await stream.WriteAsync(payload, 0, payload.Length, cancellationToken);
        await stream.FlushAsync(cancellationToken);

        byte[] buffer = new byte[1024];

        try
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            if (bytesRead > 0)
            {
                var packets = ParseResponsePayload(buffer, bytesRead);

                foreach (var packet in packets)
                {
                    Console.WriteLine($"Resent Packet: {packet.Symbol}, {packet.BuySellIndicator}, {packet.Quantity}, {packet.Price}, {packet.Sequence}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while resending packet: {ex.Message}");
        }
    }
}
