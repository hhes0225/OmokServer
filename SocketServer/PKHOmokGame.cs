using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class PKHOmokGame:PKHandler
{
    List<Room> RoomList = null;
    int StartRoomNumber;
    PacketToBytes PacketMaker = new PacketToBytes();

    public void SetRoomList(List<Room> roomList)
    {
        RoomList = roomList;
        StartRoomNumber = roomList[0].Number;
    }

    Room GetRoom(int roomNumber)
    {
        var index = roomNumber - StartRoomNumber;
        if (index < 0 || index >= RoomList.Count())
        {
            return null;
        }

        return RoomList[index];
    }

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.REQ_READY_OMOK, RequestUserReady);
    }

    public void RequestUserReady(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug("Ready request");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqReadyOmok>(packetData.BodyData);
            var room = GetRoom(reqData.RoomNumber);

            if (room == null)
            {
                MainServer.MainLogger.Debug("유효하지 않은 방");
                return;
            }

            var roomUser = room.GetUserByNetSessionID(sessionID);
            if (roomUser == null)
            {
                MainServer.MainLogger.Debug("유저 존재하지 않음");
                return;
            }

            if (roomUser.GetUserState() == UserState.Ready)
            {
                MainServer.MainLogger.Debug("유저 이미 준비상태임");
            }
            else 
            { 
                roomUser.Ready();
            }

            room.NotifyPacketUserReady(roomUser.UserID);
            ResponseUserReady(sessionID);

            if(IsGameStartPossible(room) == true)
            {
                NotifyGameStart(room);
            }

            MainServer.MainLogger.Debug($"{roomUser.UserID} is Ready");

        }
        catch(Exception ex)
        {
            MainServer.MainLogger.Error(ex.ToString());
        }
    }

    public void ResponseUserReady(string sessionID)
    {
        var resUserReady = new PKTResReadyOmok()
        {
            Result = (Int16)ERROR_CODE.NONE
        };

        var body = MemoryPackSerializer.Serialize(resUserReady);
        var sendData = PacketMaker.MakePacket(PACKETID.RES_READY_OMOK, body);

        ServerNetwork.SendData(sessionID, sendData);
    }

    public bool IsGameStartPossible(Room room)
    {
        var roomUserList = room.GetUserList();
        
        foreach (var roomUser in roomUserList)
        {
            if(roomUser.GetUserState() != UserState.Ready)
            {
                return false;
            }
        }

        return true;
    }

    public void NotifyGameStart(Room room)
    {
        var random = new Random();
        var userList = room.GetUserList();

        var randomBlackIndex = random.Next(0, room.CurrentUserCount());
        
        var packet = new PKTNtfStartOmok();
        packet.BlackUserID = userList[randomBlackIndex].UserID;
        packet.WhiteUserID = userList[room.CurrentUserCount() - randomBlackIndex-1].UserID;

        var bodyData = MemoryPackSerializer.Serialize(packet);

        var sendData = PacketMaker.MakePacket(PACKETID.NTF_START_OMOK, bodyData);
        MainServer.MainLogger.Debug("Game Start...");

        room.Broadcast("", sendData);
    }


}
