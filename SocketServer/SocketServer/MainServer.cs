using SocketLibrary;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketServer.PacketHandler;
using SocketServer.Processor;
using SocketServer.UserDir;
using SocketServer.RoomDir;

namespace SocketServer;

public class MainServer:AppServer<NetworkSession, OmokBinaryRequestInfo>, IHostedService
{
    public SuperSocket.SocketBase.Logging.ILog MainLogger;


    public ServerOption ServerOption = new ServerOption();
    SuperSocket.SocketBase.Config.IServerConfig m_Config;

    public PacketProcessor MainPacketProcessor;
    public MySqlProcessor MySqlPacketProcessor;
    public RedisProcessor RedisPacketProcessor;

    public PacketData notifyPacket = new PacketData();
    RoomManager RoomMgr;

    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<MainServer> _appLogger;

    //서버 설정 정의 & 구성 - 이벤트 핸들러 델리게이트 등록
    public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, ILogger<MainServer> logger)
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, OmokBinaryRequestInfo>())
    {
        ServerOption = serverConfig.Value;
        _appLogger = logger;
        _appLifetime = appLifetime;

        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, OmokBinaryRequestInfo>(OnPacketReceived);

        
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(AppOnStarted);
        _appLifetime.ApplicationStopped.Register(AppOnStopped);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void AppOnStarted()
    {
        _appLogger.LogInformation("Onstarted");
        InitConfig(ServerOption);
        CreateServer();

        var IsResult = base.Start();

        if (IsResult)
        {
            _appLogger.LogInformation("서버 네트워크 시작");
        }
        else
        {
            _appLogger.LogError("서버 네트워크 시작 실패");
            return;
        }
    }

    private void AppOnStopped()
    {
        MainLogger.Info("OnStopped - begin");

        StopServer();

        MainLogger.Info("OnStopped - end");
    }

    public void InitConfig(ServerOption option)
    {
        //ServerOption = option;

        m_Config = new SuperSocket.SocketBase.Config.ServerConfig
        {
            Name = option.Name,
            Ip = "Any",
            Port = option.Port,
            Mode = SocketMode.Tcp,
            MaxConnectionNumber= option.MaxConnectionNumber,
            MaxRequestLength= option.MaxRequestLength,
            ReceiveBufferSize= option.ReceiveBufferSize,
            SendBufferSize= option.SendBufferSize,

        };

        
    }

    public void CreateServer()
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
            //Start();

            MainLogger.Info("서버 생성 성공");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] 서버 생성 실패: {ex.ToString()}");
        }
    }

    public bool IsRunning(ServerState eCurState)
    {
        if (eCurState == ServerState.Running)
        {
            return true;
        }

        return false;
    }

    public void StopServer()
    {
        Stop();
        MainPacketProcessor.Destroy();
        MySqlPacketProcessor.Destroy();
        RedisPacketProcessor.Destroy();
    }
    
    public ErrorCode CreateComponent()
    {
        RoomMgr = new RoomManager(this);

        PKHandler.SendDataFunc = this.SendData;
        PKHandler.DistributeFunc = this.Distribute;
        PKHMysql.DistributeFunc = this.MySqlDistribute;
        PKHRedis.DistributeFunc = this.Distribute;
        PKHRedis.SendFunc = this.SendData;
        PKHCommon.RedisDistributeFunc = this.RedisDistribute;

        //방 기본설정 정의(몇개까지? 몇명수용?)->미리 빈 방 만들어놓는다
        Room.SendFunc = this.SendData;
        Room.SendInternalFunc = this.Distribute;
        Room.SendDbInternalFunc = this.MySqlDistribute;

        RoomManager.SendInternalFunc = this.Distribute;

        UserManager.SendInternalFunc = this.Distribute;
        UserManager.CloseConnection = this.CloseConnection;

        RoomMgr.CreateRooms();

        //packet processor 설정
        MainPacketProcessor = new PacketProcessor(MainLogger);
        MainPacketProcessor.CreateAndStart(RoomMgr.GetRoomList(), this);

        //MySQL Processor 설정
        MySqlPacketProcessor = new MySqlProcessor(MainLogger, ServerOption);
        MySqlPacketProcessor.CreateAndStart(4);

        RedisPacketProcessor = new RedisProcessor(MainLogger, ServerOption);
        RedisPacketProcessor.CreateAndStart(2);

        MainLogger.Info("CreateComponent - Success");

        return ErrorCode.None;
    }

    //이 MainServer에서 메시지 Send, 다른 클라, 서버로 메시지 전송할 때 사용
    public bool SendData(string sessionID, byte[] sendData)
    {
        //누구에게 보낼 것인가?(ID로 연결된 클라 세션에 접근)
        var session = GetSessionByID(sessionID);

        try
        {
            if(session == null)//클라 존재 X
            {
                return false;
            }

            if (sessionID == "InnerPacket")
            {
                session.Send(sendData, 0, sendData.Length);
            }

            session.Send(sendData, 0, sendData.Length);
            //SuperSocket의 라이브러리로 패킷 전송

        }
        catch(Exception ex)
        {
            MainLogger.Error($"{ex.ToString()}, {ex.StackTrace}");

            //클라에서 계속 반응 없으면(timeout) 클라-서버 연결 종료
            session.SendEndWhenSendingTimeOut();
            session.Close();
        }

        return true;
    }

    //이 Mainserver가 받은(Receive) 메시지를 프로세서의 메시지 버퍼에 등록
    public void Distribute(PacketData reqestData)
    {
        MainPacketProcessor.InsertPacket(reqestData);
    }

    public void MySqlDistribute(PacketData requestData)
    {
        MySqlPacketProcessor.InsertPacket(requestData);
    }

    public void RedisDistribute(PacketData requestData)
    {
        RedisPacketProcessor.InsertPacket(requestData);
    }

    void OnConnected(NetworkSession session)
    {
        MainLogger.Info(string.Format($"세션 번호{session.SessionID} 접속"));

        var packet = notifyPacket.MakeNTFInConnectOrDisconnectClientPacket(true, session.SessionID);
        Distribute(packet);
    }

    void OnClosed(NetworkSession session, CloseReason reason)
    {
        MainLogger.Info(string.Format($"세션 번호{session.SessionID} 접속 해제"));

        var roomList = RoomMgr.GetRoomList();

        foreach (var room in roomList)
        {
            var roomUser = room.GetUserByNetSessionID(session.SessionID);

            if (roomUser != null)
            {
                room.RemoveUser(roomUser);
                room.NotifyPacketLeaveUser(roomUser.UserID);
                //게임 중에 누가 나가면 남은 사람이 승리
                if (room.OmokBoard.GameFinish == false)
                {
                    room.NotifyEndOmok(room.GetUserList()[0].NetSessionID);
                }
                break;
            }
        }

        var packet = notifyPacket.MakeNTFInConnectOrDisconnectClientPacket(false, session.SessionID);
        Distribute(packet);
    }

    //받은 패킷 header Deserialize, body 분리(아직 Deserialize X)
    //packetID에 맞는 핸들러 호출해서 그 핸들러 함수에서 body 클래스에 맞게 deserialize
    void OnPacketReceived(NetworkSession session, OmokBinaryRequestInfo requestInfo)
    {
        MainLogger.Debug(string.Format($"세션 번호 {session.SessionID}, 받은 데이터 크기 {requestInfo.Body.Length}, " +
            $"ThreadID: {System.Threading.Thread.CurrentThread.ManagedThreadId}"));

        var packet = new PacketData();
        packet.SessionID = session.SessionID;
        packet.PacketSize = requestInfo.Size;
        packet.PacketID = requestInfo.PacketID;
        packet.BodyData = requestInfo.Body;

        //Client에서 Internal packet이면 잘못 전송된 패킷임
        if ((int)requestInfo.PacketID == (int)PACKETID.ReqDbLogin)
        {
            //메인 프로세서 버퍼에 등록(처리요청)
            RedisDistribute(packet);
            return;
        }
        if ((int)requestInfo.PacketID >=(int)PACKETID.ReqBegin || (int)requestInfo.PacketID >= (int)PACKETID.ReqEnd)
        {

            //메인 프로세서 버퍼에 등록(처리요청)
            Distribute(packet);
            return;
        }

    }

    public void CloseConnection(string sessionID)
    {
        var sessions = GetAllSessions();

        foreach(var session in sessions)
        {
            if(session.SessionID == sessionID)
            {
                //NTFForceDisconnection(sessionID);

                session.Close();

                return;
            }
        }
    }
}

public class NetworkSession:AppSession<NetworkSession, OmokBinaryRequestInfo>
{
}