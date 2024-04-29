using System;

namespace ChatServer;

class Program
{
    //dotnet ChatServer.dll --uniqueID 1 --roomMaxCount 16 --roomMaxUserCount 4 --roomStartNumber 1 --maxUserCount 100
    static void Main(string[] args)
    {
        //ParseCommandLine 메서드 구현 필요
        var serverOption = ParseCommandLine(args);

        //MainServer 클래스 구현 필요
        var serverApp = new MainServer();
        serverApp.InitConfig(serverOption);
        serverApp.CreateAndStartServer();//server start 전 setup(Awake같은 느낌)&start

        MainServer.MainLogger.Info("Press q to shutdown the server");

        while (true)
        {
            System.Threading.Thread.Sleep(50);//50ms

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                //ReadKey 매개변수 true: 사용자가 누른 키는 콘솔 창에 표시 X

                if (key.KeyChar == 'q')//입력 키가 q일때만 종료
                {
                    Console.WriteLine("Server Terminated---");
                    serverApp.StopServer();
                    break;
                }
            }
        }
    }

    //ChatServerOption 클래스 구현 필요
    static ChatServerOption ParseCommandLine(string[] args)
    {
        //main 실행 시 받은 매개변수들을 알아서 ChatServerOption의 멤버변수들로 파싱해줌
        var result = CommandLine.Parser.Default.ParseArguments<ChatServerOption>(args) as CommandLine.Parsed<ChatServerOption>;

        if(result == null)
        {
            Console.WriteLine("Failed Command Line");
            return null;
        }

        return result.Value;
    }
}