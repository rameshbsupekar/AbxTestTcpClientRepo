using Moq;
using System.Net.Sockets;

namespace Abx.Client.Library.Tests;

[TestClass]
public class RealTimeTcpClientTests
{
    [TestMethod]
    public async Task StartAsync_ShouldConnectToServer()
    {
        // Arrange
        var mockTcpClient = new Mock<ITcpClient>();
        var client = new RealTimeTcpClient(mockTcpClient.Object, "localhost", 12345);

        // Act
        await client.StartAsync();

        // Assert
        mockTcpClient.Verify(m => m.ConnectAsync("localhost", 12345), Times.Once);
    }

    [TestMethod]
    public async Task StartAsync_ShouldConnectAndStartReceiving()
    {
        // Arrange
        var mockTcpClient = new Mock<ITcpClient>();
        mockTcpClient.Setup(m => m.ConnectAsync(It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

        var client = new RealTimeTcpClient(mockTcpClient.Object, "localhost", 12345);

        // Act
        await client.StartAsync(CancellationToken.None);

        // Assert
        mockTcpClient.Verify(m => m.ConnectAsync("localhost", 12345), Times.Once);
    }

    [TestMethod]
    public void Stop_ShouldCloseConnection()
    {
        // Arrange
        var mockTcpClient = new Mock<ITcpClient>();
        var client = new RealTimeTcpClient(mockTcpClient.Object, "localhost", 12345);

        // Act
        client.Stop();

        // Assert
        mockTcpClient.Verify(m => m.Close(), Times.Once);
    }

    [TestMethod]
    public async Task StartAsync_ShouldHandleConnectionFailure()
    {
        // Arrange
        var mockTcpClient = new Mock<ITcpClient>();
        mockTcpClient.Setup(m => m.ConnectAsync(It.IsAny<string>(), It.IsAny<int>()))
                     .ThrowsAsync(new SocketException());

        var client = new RealTimeTcpClient(mockTcpClient.Object, "localhost", 12345);

        // Act
        try
        {
            await client.StartAsync(CancellationToken.None);
        }
        catch (SocketException)
        {
            // Assert
            Assert.Fail("StartAsync should not throw a SocketException.");
        }
    }

    [TestMethod]
    public void Stop_ShouldNotThrowIfNotStarted()
    {
        // Arrange
        var mockTcpClient = new Mock<ITcpClient>();
        var client = new RealTimeTcpClient(mockTcpClient.Object, "localhost", 12345);

        // Act & Assert
        client.Stop();
    }

    [TestMethod]
    public async Task StartAsync_ShouldHandleEmptyStream()
    {
        // Arrange
        var mockStream = new Mock<INetworkStream>();
        mockStream.Setup(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(0); // Simulate empty stream

        var mockTcpClient = new Mock<ITcpClient>();
        mockTcpClient.Setup(m => m.GetStream()).Returns(mockStream.Object);

        var client = new RealTimeTcpClient(mockTcpClient.Object, "localhost", 12345);

        // Act
        await client.StartAsync(CancellationToken.None);

        // Assert
        mockStream.Verify(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()), Times.Once);
    }
}
