// See https://aka.ms/new-console-template for more information
using Abx.Client.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Abx.Client.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create a HostBuilder to set up dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Add appsettings.json to the configuration
                config.AddJsonFile(".\\Program\\appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                var configuration = context.Configuration;

                // Manually bind TcpSettings and register it as a singleton
                var tcpSettings = configuration.GetSection("TcpSettings").Get<TcpSettings>();
                services.AddSingleton<ITcpSettings>(tcpSettings ?? throw new InvalidOperationException("TcpSettings configuration is missing."));
                services.AddSingleton<ITcpClient, TcpClientWrapper>();
            })
            .Build();

        // Resolve the ITcpClient and run the application
        var tcpClient = host.Services.GetRequiredService<ITcpClient>();
        var tcpSettings = host.Services.GetRequiredService<ITcpSettings>();

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            // Connect to the server
            Console.WriteLine($"Connecting to {tcpSettings.Host}:{tcpSettings.Port}...");
            await tcpClient.ConnectAsync(tcpSettings.Host, tcpSettings.Port);

            // Stream all packets
            Console.WriteLine("Streaming all packets...");
            await ((TcpClientWrapper)tcpClient).StreamAllPacketsAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            tcpClient.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
