using System;

namespace SocketServer;

class Program
{
    static void Main(string[] args)
    {
        //ParseCommandLine 메서드
        var serverOption = ParseCommandLine(args);

        //MainServer 클래스 변수 관련 설정
        var serverApp = new MainServer();
        serverApp.InitConfig(serverOption);
        serverApp.CreateAndStartServer();

        serverApp.MainLogger.Info("Press q to shutdown the server");

        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.KeyChar == 'q')
                {
                    Console.WriteLine("Server Terminated---");
                    serverApp.StopServer();
                    break;
                }
            }
        }
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
}