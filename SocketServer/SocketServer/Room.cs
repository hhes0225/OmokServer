using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public partial class Room
{
    public int Index { get; set; }
    public int Number {  get; set; }
    public PacketToBytes PacketMaker = new PacketToBytes();

    public OmokRule OmokBoard = new OmokRule();

    public int MaxUserCount = 0;

    List <RoomUser> UserList = new List<RoomUser> ();

    public static Func<string, byte[], bool> SendFunc;
    //MainServer에서 참조할 함수 지정
    public static Action<PacketData> SendInternalFunc;
    public static Action<PacketData> SendDbInternalFunc;

    public bool IsRoomUsing = false;
    public DateTime FirstEntryTime { get; private set; }
    public DateTime GameStartTime {  get; private set; }

    private int RoomTimeSpan, GameTimeSpan;

    public void Init(int index, int number, int maxUserCount) 
    { 
        Index = index;
        Number = number;
        MaxUserCount = maxUserCount;

    }

    public void InitTimeSpan(int roomTimeSpan, int gameTimeSpan)
    {
        RoomTimeSpan = roomTimeSpan;
        GameTimeSpan = gameTimeSpan;
    }

    public List<RoomUser> GetUserList()
    {
        return UserList;
    }

    public RoomUser GetUser(string userID)
    {
        return UserList.Find(x => x.UserID == userID);
    }

    public RoomUser GetUserByNetSessionID(string netSessionID)
    {
        return UserList.Find(x => x.NetSessionID == netSessionID);
    }

    public int CurrentUserCount()
    {
        return UserList.Count;
    }


    public void Broadcast(string excludeNetSessionID, byte[] sendPacket)
    {
        foreach(var user in UserList)
        {
            //자기 자신을 제외한 방 안 모든 유저에게 전송
            if (user.NetSessionID == excludeNetSessionID)
            {
                continue;
            }

            SendFunc(user.NetSessionID, sendPacket);//SendData
        }
    }
}

