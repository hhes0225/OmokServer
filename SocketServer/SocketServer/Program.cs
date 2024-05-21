using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SocketServer;

class Program
{
    static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        })
        .ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddConsole();
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
            services.AddHostedService<MainServer>();
        })
        .Build();

        var lifetime = host.Services.GetService<IHostApplicationLifetime>();

        // 별도의 스레드에서 키보드 입력을 감지합니다.
        Task.Run(() =>
        {
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                {
                    lifetime.StopApplication();
                    break;
                }
            }
        });


        await host.RunAsync();
    }
    

    static ServerOption ParseCommandLine(string[] args)
    {
        var result = CommandLine.Parser.Default.ParseArguments<ServerOption>(args) as CommandLine.Parsed<ServerOption>;

        if (result == null)
        {
            Console.WriteLine("Failed Command Line");
            return null;
        }

        return result.Value;
    }

    static ServerOption ParseJsonFile(string path)
    {
        try
        {
            var jsonString = File.ReadAllText(path);
            var serverOption = JsonSerializer.Deserialize<ServerOption>(jsonString);

            return serverOption;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse JSON file: {ex.Message}");
            return null;
        }
    }
}
