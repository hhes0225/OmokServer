using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class RoomManager
{
    List<Room> RoomList = new List<Room>();
    MainServer ServerNetwork;

    private Timer _checkRoomStateTimer;
    private int RoomTimeSpan, GameTimeSpan, GameTurnTimeSpan;


    public RoomManager(MainServer mainServer)
    {
        ServerNetwork = mainServer;

    }


    public void SetTimeSpans(int roomTimeSpan, int gameTimeSpan, int gameTurnTimeSpan)
    {
        RoomTimeSpan = roomTimeSpan;
        GameTimeSpan = gameTimeSpan;
        GameTurnTimeSpan = gameTurnTimeSpan;
    }

    public void CreateRooms()
    {
        var maxRoomCount = ServerNetwork.ServerOption.RoomMaxCount;
        var startNumber = ServerNetwork.ServerOption.RoomStartNumber;
        var maxUserCount = ServerNetwork.ServerOption.RoomMaxUserCount;

        for(int i=0; i<maxRoomCount; i++)
        {
            var roomNubmer = startNumber + i;
            var room = new Room();
            room.Init(i, roomNubmer, maxUserCount);
            RoomList.Add(room);
        }

        InitAndStartTimer(0, 5000);
        SetTimeSpans(1, 1, 0);
    }


    public void InitAndStartTimer(int startTime, int interval)
    {
        TimerCallback callback= new TimerCallback(SendRoomCheckPkt);
        _checkRoomStateTimer = new Timer(callback, null, startTime, interval);
    }

    public void SendRoomCheckPkt(object state)
    {
        var ntfPkt = new PKTNtfInnerRoomCheck();
        var body = MemoryPackSerializer.Serialize(ntfPkt);

        var internalPacket = new PacketData();
        internalPacket.Assign((int)PACKETID.NTF_INNER_ROOM_CHECK, body);

        ServerNetwork.Distribute(internalPacket);
    }

    public void CheckHeartBeat(int beginIndex, int endIndex)
    {
        if (endIndex > RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for(int i = beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].IsRoomUsing == false)
            {
                continue;
            }

            //Room의 체크방크리에이트
            //ROom의 체크게임스타트
        }
    }

    //마찬가지로 ROom의 체크게임스타트 함수

    public List<Room> GetRoomList()
    {
        return RoomList;
    }

}
