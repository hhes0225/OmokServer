using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public partial class Room
{

    public bool AddUser(string userID, string netSessionID)
    {
        if (GetUser(userID) != null)
        {
            return false;
        }

        var newRoomUser = new RoomUser();
        newRoomUser.Set(userID, netSessionID);
        UserList.Add(newRoomUser);

        ActivateRoom();

        return true;
    }

    public void ActivateRoom()
    {
        if (IsRoomUsing == false)
        {
            IsRoomUsing = true;
            FirstEntryTime = DateTime.Now;
        }

    }
    public void InactivateRoom()
    {
        if (IsRoomUsing == true)
        {
            IsRoomUsing = false;
            FirstEntryTime = DateTime.MinValue;
        }
    }

    public bool IsRoomCreatedButNotPlaying(DateTime curTime)
    {
        var diff = curTime - FirstEntryTime;

        if ((int)diff.TotalMinutes >= RoomTimeSpan && OmokBoard.GameFinish == true)
        {
            return true;
        }

        return false;
    }

    public bool IsGamePlayingTooLong(DateTime curTime)
    {
        var diff = curTime - GameStartTime;

        if ((int)diff.TotalHours >= GameTimeSpan && OmokBoard.GameFinish == false)
        {
            return true;
        }

        return false;
    }



    public void RemoveAllUser()
    {
        for (int i = UserList.Count - 1; i >= 0; i--)
        {
            if (UserList[i] != null)
            {
                RemoveUser(UserList[i]);
            }
        }

    }

    public bool RemoveUser(RoomUser user)
    {
        NotifyPacketLeaveUser(user.UserID);
        Console.WriteLine($"{user.UserID}: {DateTime.Now}");

        //유저패킷 전달
        var internalPacket = new PacketData();
        var data = new PKTInternalNtfRoomLeave()
        {
            UserID = user.NetSessionID,
            RoomNumber = Number
        };

        var bodyData = MemoryPackSerializer.Serialize(data);
        internalPacket.Assign((Int16)PACKETID.NtfInRoomLeave, bodyData);
        SendInternalFunc(internalPacket);


        //여기가 기존
        if (user == null)
        {
            return false;
        }

        var result = UserList.Remove(user);

        if (CurrentUserCount() == 0)
        {
            InactivateRoom();
        }

        return result;
    }

    public void NotifyPacketUserList(string userNetSessionID)
    {
        var packet = new CSBaseLib.PKTNtfRoomUserList();

        foreach(var user in UserList)
        {
            packet.UserIDList.Add(user.UserID);
        }

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NtfRoomUserList, bodyData);

        SendFunc(userNetSessionID, sendPacket);
    }

    public void NotifyPacketNewUser(string newUserNetSessionID, string newUserID)
    {
        var packet = new CSBaseLib.PKTNtfRoomNewUser();
        packet.UserID = newUserID;

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NtfRoomNewUser, bodyData);

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
        var sendPacket = PacketMaker.MakePacket(PACKETID.NtfRoomLeaveUser, bodyData);

        //방 전체에게 뿌리기 -> Broadcast
        Broadcast("", sendPacket);
    }
}
