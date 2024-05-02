using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class Room
{
    public int Index { get; set; }
    public int Number {  get; set; }
    public PacketToBytes PacketMaker = new PacketToBytes();

    int MaxUserCount = 0;

    List <RoomUser> UserList = new List<RoomUser> ();

    public static Func<string, byte[], bool> NetSendFunc;
    //MainServer에서 참조할 함수 지정

    public void Init(int index, int number, int maxUserCount) 
    { 
        Index = index;
        Number = number;
        MaxUserCount = maxUserCount;
    }

    public bool AddUser(string userID, string netSessionID)
    {
        if (GetUser(userID) != null)
        {
            return false;
        }

        var newRoomUser = new RoomUser();
        newRoomUser.Set(userID, netSessionID);
        UserList.Add(newRoomUser);

        return true;
    }

    public bool RemoveUser(RoomUser user)
    {
        return UserList.Remove(user);
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

    public void NotifyPacketUserLIst(string userNetSessionID)
    {
        var packet = new CSBaseLib.PKTNtfRoomUserList();

        foreach(var user in UserList)
        {
            packet.UserIDList.Add(user.UserID);
        }

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_ROOM_USER_LIST, bodyData);

        NetSendFunc(userNetSessionID, sendPacket);
    }

    public void NotifyPacketNewUser(string newUserNetSessionID, string newUserID)
    {
        var packet = new CSBaseLib.PKTNtfRoomNewUser();
        packet.UserID = newUserID;

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_ROOM_NEW_USER, bodyData);

        Broadcast(newUserNetSessionID, sendPacket);
    }

    public void NotifyPacketLeaveUser(string userID)
    {
        if(CurrentUserCount() == 0) 
        {
            return;
        }

        var packet = new CSBaseLib.PKTNtfRoomLeaveUser();
        packet.UserID = userID;

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_ROOM_LEAVE_USER, bodyData);

        //방 전체에게 뿌리기 -> Broadcast
        Broadcast("", sendPacket);
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

            NetSendFunc(user.NetSessionID, sendPacket);
        }
    }
}

public class RoomUser
{
    public string UserID { get; private set; }
    public string NetSessionID { get; private set; } 

    public void Set(string userID, string netSessionID)
    {
        UserID = userID;
        NetSessionID = netSessionID;
    }
}
