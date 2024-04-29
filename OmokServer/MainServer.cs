using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;

using CSBaseLib;
using System.Net.Http.Headers;

namespace ChatServer;

//NetworkSession 클래스 구현 필요: 새로운 연결 발생 시 세션 객체 생성
//EFBinaryRequestInfo 클래스 구현 필요: 패킷 형식 규칙 정의(
public class MainServer:AppServer<NetworkSession, EFBinaryRequestInfo>
{
    public static ChatServerOption ServerOption;
    public static SuperSocket.SocketBase.Logging.ILog MainLogger;

    SuperSocket.SocketBase.Config.IServerConfig m_Config;//서버 설정 정의&구성

    PacketProcessor MainPacketProcessor = new PacketProcessor();
    RoomManager RoomMgr = new RoomManager();

    //ReceiveFilter 클래스 구현 필요
    public MainServer()
        :base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
    {
        //델리게이트-메서드에 대한 참조 저장 / 이벤트 처리 상황에서 사용
        //OnConnected, OnClosed, OnPacketReceived 메서드 구현 필요
        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);//접속 연결 시 호출
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);//접속 해제 시 호출
        NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(OnPacketReceived);//패킷 recv 시 호출
    }

    public void InitConfig(ChatServerOption option)
    {
        ServerOption = option;

        m_Config = new SuperSocket.SocketBase.Config.ServerConfig
        {
            Name = option.Name,
            Ip = "Any",
            Port = option.Port,
            Mode = SocketMode.Tcp,
            MaxConnectionNumber = option.MaxConnectionNumber,
            MaxRequestLength = option.MaxRequestLength,
            ReceiveBufferSize = option.ReceiveBufferSize,
            SendBufferSize = option.SendBufferSize,
        };
    }

    public void CreateAndStartServer()
    {
        try
        {
            bool bResult = Setup(new RootConfig(), m_Config, logFactory: new ConsoleLogFactory());

            if (bResult == false)
            {
                Console.WriteLine("[Error] 서버 네트워크 설정 실패");
                return;
            }
            else
            {
                MainLogger = base.Logger;
                MainLogger.Info("서버 초기화 성공");
            }

            CreateComponent();
            Start();

            MainLogger.Info("서버 생성 성공");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Error] 서버 생성 실패: {ex.ToString()}");
        }
    }

    public void StopServer()
    {
        Stop();

        //MainPacketProcessor 클래스 정의 필요
        MainPacketProcessor.Destroy();
        
    }

    //ERROR_CODE enum 정의 필요(PacketDef)


    //SuperSocket에서 사용할 객체들을 만들고 있음
    public ERROR_CODE CreateComponent()
    {
        //Room 클래스 정의 필요
        Room.NetSendFunc = this.SendData;
        RoomMgr.CreateRooms();
        //사용자가 방을 만들 때마다 방이 만들어지는게 아니라,
        //서버 시작과 동시에 룸이 만들어지고, 사용자가 입주하는 느낌.
        //사용자가 빠져나가면 빈 방에 다른 사용자가 들어온다. -> 재사용


        //채팅서버는 패킷 처리를 one-thread로 하고 있음
        MainPacketProcessor = new PacketProcessor();
        MainPacketProcessor.CreateAndStart(RoomMgr.GetRoomList(), this);

        MainLogger.Info("CreateComponent - Success");

        return ERROR_CODE.NONE;
    }

    public bool SendData(string sessionID, byte[] sendData)
    {
        //어떤 클라이언트와의 session인지 구분 필요
        var session = GetSessionByID(sessionID);

        try
        {
            if (session == null)//찾는 클라이언트와의 연결 세션이 없음
            {
                return false;
            }

            session.Send(sendData, 0, sendData.Length);
        }
        catch(Exception ex)
        {
            //TimeoutException 예외 발생할 수 있음
            MainServer.MainLogger.Error($"{ex.ToString()}, {ex.StackTrace}");

            session.SendEndWhenSendingTimeOut();//timeout이면 클라이언트-서버 연결 종료
            session.Close();
        }

        return true;
    }

    //ServerPacketData 정의 필요

    //Packet 처리 스레드로 전달
    public void Distribute(ServerPacketData requestPacket)
    {
        MainPacketProcessor.InsertPacket(requestPacket);
    }

    void OnConnected(NetworkSession session)
    {
        //옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 차단.
        //-> OnConnected 함수 호출되지 않음
        MainLogger.Info(string.Format($"세션 번호{session.SessionID} 접속"));

        //ServerPacketData 클래스 정의 필요
        var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID);
        Distribute(packet);
    }

    void OnClosed(NetworkSession session, CloseReason reason)
    {
        MainLogger.Info(string.Format($"세션 번호{session.SessionID} 접속해제: {reason.ToString()}"));

        //ServerPacketData 클래스 정의 필요
        var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(false, session.SessionID);
        Distribute(packet);
    }

    void OnPacketReceived(NetworkSession session, EFBinaryRequestInfo requestInfo)
    {
        MainLogger.Debug(string.Format($"세션 번호 {session.SessionID}, 받은 데이터 크기 {requestInfo.Body.Length}" +
            $"ThreadID: {System.Threading.Thread.CurrentThread.ManagedThreadId}"));

        var packet = new ServerPacketData();
        packet.SessionID = session.SessionID;
        packet.PacketSize = requestInfo.Size;
        packet.PacketID = requestInfo.PacketID;//서버가 해야 하는 동작 요청
        packet.Type = requestInfo.Type;
        packet.BodyData = requestInfo.Body;

        Distribute(packet);
    }

}


public class NetworkSession:AppSession<NetworkSession, EFBinaryRequestInfo>
{

}
