using Abx.Client.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace Abx.Client.Library.Tests
{
    [TestClass]
    public class TcpClientWrapperTests
    {
        [TestMethod]
        public async Task ConnectAsync_ShouldConnectToServer()
        {
            // Arrange
            var tcpClient = new TcpClientWrapper();

            // Act & Assert
            await tcpClient.ConnectAsync("localhost", 12345);
        }

        [TestMethod]
        public async Task StreamAllPacketsAsync_ShouldReceivePackets()
        {
            // Arrange
            var mockStream = new Mock<INetworkStream>();
            mockStream.Setup(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(0); // Simulate server closing connection

            var mockTcpClient = new Mock<ITcpClient>();
            mockTcpClient.Setup(m => m.GetStream()).Returns(mockStream.Object);

            var tcpClientWrapper = new TcpClientWrapper();

            // Act
            await tcpClientWrapper.StreamAllPacketsAsync(CancellationToken.None);

            // Assert
            mockStream.Verify(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ResendPacketAsync_ShouldRequestSpecificPacket()
        {
            // Arrange
            var mockStream = new Mock<INetworkStream>();
            var mockTcpClient = new Mock<ITcpClient>();
            mockTcpClient.Setup(m => m.GetStream()).Returns(mockStream.Object);

            var tcpClientWrapper = new TcpClientWrapper();

            // Act
            await tcpClientWrapper.ResendPacketAsync(5, CancellationToken.None);

            // Assert
            mockStream.Verify(m => m.WriteAsync(It.IsAny<byte[]>(), 0, 2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePayload_ShouldThrowExceptionForInvalidCallType()
        {
            // Arrange
            var tcpClientWrapper = new TcpClientWrapper();

            // Act
            var payload = tcpClientWrapper.CreatePayload(99); // Invalid callType
        }

        [TestMethod]
        public async Task StreamAllPacketsAsync_ShouldHandleServerDisconnection()
        {
            // Arrange
            var mockStream = new Mock<INetworkStream>();
            mockStream.SetupSequence(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(10) // First read succeeds
                      .ReturnsAsync(0); // Server closes connection

            var mockTcpClient = new Mock<ITcpClient>();
            mockTcpClient.Setup(m => m.GetStream()).Returns(mockStream.Object);

            var tcpClientWrapper = new TcpClientWrapper();

            // Act
            await tcpClientWrapper.StreamAllPacketsAsync(CancellationToken.None);

            // Assert
            mockStream.Verify(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ResendPacketAsync_ShouldThrowExceptionForInvalidSequence()
        {
            // Arrange
            var tcpClientWrapper = new TcpClientWrapper();

            // Act
            await tcpClientWrapper.ResendPacketAsync(-1, CancellationToken.None); // Invalid sequence
        }

        [TestMethod]
        public async Task StreamAllPacketsAsync_ShouldRespectCancellationToken()
        {
            // Arrange
            var mockStream = new Mock<INetworkStream>();
            mockStream.Setup(m => m.ReadAsync(It.IsAny<byte[]>(), 0, 1024, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(10);

            var mockTcpClient = new Mock<ITcpClient>();
            mockTcpClient.Setup(m => m.GetStream()).Returns(mockStream.Object);

            var tcpClientWrapper = new TcpClientWrapper();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => tcpClientWrapper.StreamAllPacketsAsync(cts.Token));
        }
    }
}
