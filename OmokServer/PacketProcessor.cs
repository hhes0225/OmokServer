using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ChatServer;

//이 프로젝트는 싱글 스레드.
//즉, 패킷을 주고 받는 스레드가 멀티가 아니라 단 하나라는 것이다.
//(공장에 일꾼 하나같은 느낌)
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
    System.Threading.Thread ProcessThread = null;

    //receive 쪽에서 처리하지 않아도 Post에서 blocking되지 않음.
    //BufferBlock<T>(DataflowBlockOptions)에서 DataflowBlockOptions의 BoundedCapacity 로 버퍼 가능 수 지정.
    //BoundedCapacity 보다 크게 쌓이면 blocking
    BufferBlock<ServerPacketData> MsgBuffer = new BufferBlock<ServerPacketData>();
    //BufferBlock이라는 일종의 큐

    UserManager UserMgr = new UserManager();

    Tuple<int, int> RoomNumberRange= new Tuple<int, int>(-1,-1);
    List<Room> RoomList = new List<Room>();

    Dictionary<int, Action<ServerPacketData>> PacketHandleMap = new Dictionary<int, Action<ServerPacketData>>();
    PKHCommon CommonPacketHandler=new PKHCommon();//
    PKHRoom RoomPacketHandler = new PKHRoom();

    
    public void CreateAndStart(List<Room> roomList, MainServer mainServer)
    {
        var maxUserCount = MainServer.ServerOption.RoomMaxCount*MainServer.ServerOption.RoomMaxUserCount;
        //(총 허용 가능 유저 수: 방 개수*방 인원 제한 수)
        UserMgr.Init(maxUserCount);

        RoomList = roomList;
        var minRoomNum = RoomList[0].Number;
        var maxRoomNum = RoomList[0].Number+RoomList.Count()-1;
        RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);

        RegisterPacketHandler(mainServer);

        IsThreadRunning = true;
        ProcessThread = new System.Threading.Thread(this.Process);//Process 함수 실행됨
        //스레드 생성
        ProcessThread.Start();

    }

    public void Destroy()
    {
        IsThreadRunning = false;
        MsgBuffer.Complete();
    }

    public void InsertPacket(ServerPacketData data)
    {
        MsgBuffer.Post(data);
    }

    void RegisterPacketHandler(MainServer serverNetwork)
    {
        CommonPacketHandler.Init(serverNetwork, UserMgr);
        CommonPacketHandler.RegisterPacketHandler(PacketHandleMap);

        RoomPacketHandler.Init(serverNetwork, UserMgr);
        RoomPacketHandler.SetRoomList(RoomList);
        RoomPacketHandler.RegisterPacketHandler(PacketHandleMap);
    }

    
    void Process()
    {
        if(IsThreadRunning)//스레드 동작하는동안 계속 실행됨
        {
            try
            {
                //supersocket receive에서 패킷 처리하는 쪽으로 데이터 넘길 때 사용
                var packet = MsgBuffer.Receive();
                //Buffer에서 데이터 빼올때: receive했을 때 아무 데이터도 안오면 stop
                //Buffer에 데이터 넣을 때는 post 호출
                //BufferBlock : Thread-safe
               

                if (PacketHandleMap.ContainsKey(packet.PacketID))
                {
                    PacketHandleMap[packet.PacketID](packet);
                }
                else//요청번호:함수 매핑 딕셔너리에서 요청번호 키의 검색결과가 없다면
                {
                    System.Diagnostics.Debug.WriteLine($"세션 번호: {packet.SessionID}, PacketID {packet.PacketID}," +
                        $"받은 데이터 크기: {packet.BodyData.Length}");
                }
            }
            catch (Exception ex)
            {
                IsThreadRunning.IfTrue(() => MainServer.MainLogger.Error(ex.ToString()));
            }
        }
    }
        
}
