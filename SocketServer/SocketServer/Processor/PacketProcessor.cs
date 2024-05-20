using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SocketServer.PacketHandler;
using SocketServer.UserDir;
using SocketServer.RoomDir;

namespace SocketServer.Processor;
//이 프로젝트는 싱글 스레드.
//즉, 패킷을 주고 받는 스레드가 멀티가 아니라 단 하나라는 것이다.(공장에 일꾼 하나같은 느낌)

//PacketProcessor 클래스는 이 프로세스가 해야하는 작업을 정의한다.
//(처음에 스레드 1개 생성, 나중에 소멸,

//주 목적이 ""패킷 전달""이므로 패킷을 가지고 있다가 cli or srv에 전달
//    -> 이것은 MsgBuffer를 읽고 쓰는 것에 의해 이루어진다
// Distribute로 receive된 패킷을 msgBuffer에 등록(Post)
// msgBuffer에 받은 값을 실제 사용하기 위해 변수로 받음(Receive)

// * 단 이는 Receive에 해당하는 것일 뿐,
// 서버에서 send 할 경우 MsgBuffer를 사용하지 않고 바로 클라이언트로 Send한다.

// 패킷을 Receive할 때에는 패킷 해석이 필요하지만(동작 수행을 위해)
// Send할 때는 그냥 결과 여부만 Send하는 것이므로 패킷 해석이 필요하지 않음. 그냥 binary화 시켜서 보내면 된다.

public class PacketProcessor
{
    bool IsThreadRunning = false;
    Thread ProcessThread = null;

    //큐: 헤더 정보는 그대로, body정보는 serialize된 byte배열
    BufferBlock<PacketData> MsgBuffer = new BufferBlock<PacketData>();

    UserManager UserMgr;

    Tuple<int, int> RoomNumberRange = new Tuple<int, int>(-1, -1);
    List<Room> RoomList = new List<Room>();//룸 리스트 생성

    //패킷 핸들러 등록
    Dictionary<int, Action<PacketData>> PacketHandlerMap = new Dictionary<int, Action<PacketData>>();
    PKHCommon CommonPacketHandler = new PKHCommon();
    PKHRoom RoomPacketHandler = new PKHRoom();
    PKHOmokGame OmokPacketHandler = new PKHOmokGame();
    PKHHeartbeat HeartbeatHandler = new PKHHeartbeat();

    SuperSocket.SocketBase.Logging.ILog ProcessorLogger;

    public PacketProcessor(SuperSocket.SocketBase.Logging.ILog logger)
    {
        ProcessorLogger = logger;
        UserMgr = new UserManager(logger);
    }

    public void CreateAndStart(List<Room> roomList, MainServer mainServer)
    {
        //유저 관련 정보 초기화
        //(총 허용 가능 유저 수: 방 개수*방 인원 제한 수)
        var maxUserCount = mainServer.ServerOption.RoomMaxCount * mainServer.ServerOption.RoomMaxUserCount;
        UserMgr.Init(maxUserCount, 0, 500);

        //방 관련 정보 초기화
        RoomList = roomList;
        var minRoomNum = RoomList[0].Number;
        var maxRoomNum = RoomList[0].Number + RoomList.Count() - 1;
        RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);

        //프로세서 패킷 핸들러 등록
        RegisterPacketHandler(mainServer);

        //스레드 시작 관련 세팅
        IsThreadRunning = true;
        ProcessThread = new Thread(Process);
        ProcessThread.Start();
    }

    public void Destroy()
    {
        IsThreadRunning = false;
        MsgBuffer.Complete();
        //Complete: BufferBlock 클래스의 메서드.
        //데이터 흐름 블록에 더 이상 데이터가 추가되지 않음(입력 안받음), 완료 상태 전환
        //데이터 흐름 블록의 생명주기 관리하는 데 중요한 역할
    }

    public void InsertPacket(PacketData data)
    {
        //버퍼에 패킷데이터 삽입.
        MsgBuffer.Post(data);
    }

    void RegisterPacketHandler(MainServer serverNetwork)
    {
        CommonPacketHandler.Init(serverNetwork.MainLogger, UserMgr);
        CommonPacketHandler.RegisterPacketHandler(PacketHandlerMap);

        RoomPacketHandler.Init(serverNetwork.MainLogger, UserMgr);
        RoomPacketHandler.SetRoomList(RoomList);
        RoomPacketHandler.RegisterPacketHandler(PacketHandlerMap);

        OmokPacketHandler.Init(serverNetwork.MainLogger, UserMgr);
        OmokPacketHandler.SetRoomList(RoomList);
        OmokPacketHandler.RegisterPacketHandler(PacketHandlerMap);

        HeartbeatHandler.Init(serverNetwork.MainLogger, UserMgr);
        HeartbeatHandler.RegisterPacketHandler(PacketHandlerMap);
    }

    void Process()
    {
        //스레드 동작하는 동안 계속 실행됨
        while (IsThreadRunning)
        {
            try
            {
                //패킷 receive
                var packet = MsgBuffer.Receive();

                if (PacketHandlerMap.ContainsKey(packet.PacketID))
                {
                    PacketHandlerMap[packet.PacketID](packet);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"MainProcessor - 세션 번호: {packet.SessionID}, PacketID {packet.PacketID}," +
                        $"받은 데이터 크기: {packet.BodyData.Length}");
                }
            }
            catch (Exception ex)
            {
                IsThreadRunning.IfTrue(() => ProcessorLogger.Error(ex.ToString()));
            }
        }
    }
}
