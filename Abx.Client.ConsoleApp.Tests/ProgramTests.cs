using Abx.Client.Library;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Abx.Client.ConsoleApp.Tests;

[TestClass]
public class ProgramTests
{
    [TestMethod]
    public void DependencyInjection_ShouldResolveDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITcpSettings>(new TcpSettings(12345, "localhost"));
        services.AddSingleton<ITcpClient, TcpClientWrapper>();

        var provider = services.BuildServiceProvider();

        // Act
        var tcpClient = provider.GetService<ITcpClient>();
        var tcpSettings = provider.GetService<ITcpSettings>();

        // Assert
        Assert.IsNotNull(tcpClient);
        Assert.IsNotNull(tcpSettings);
    }

    [TestMethod]
    public void DependencyInjection_ShouldThrowIfTcpSettingsMissing()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => services.BuildServiceProvider().GetService<ITcpSettings>());
    }
}
