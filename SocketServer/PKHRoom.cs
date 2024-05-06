using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class PKHRoom:PKHandler
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
        if(index<0 || index >= RoomList.Count())
        {
            return null;
        }

        return RoomList[index];
    }
    
    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.REQ_ROOM_ENTER, RequestRoomEnter);
        packetHandlerMap.Add((int)PACKETID.REQ_ROOM_LEAVE, RequestRoomLeave);
        packetHandlerMap.Add((int)PACKETID.REQ_ROOM_CHAT, RequestChat);
    }

    public void RequestRoomEnter(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug("Request Room Enter");

        try
        {
            var user = UserMgr.GetUser(sessionID);

            if (user == null || user.IsSessionConfirm(sessionID)==false)
            {
                ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                return;
            }

            //이미 방에 들어가있는 상태에서 다른 방 입장하려고 할 때
            if (user.IsInRoom())
            {
                ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                return;
            }

            //Receive한 리퀘스트 데이터 Deserialize
            var reqData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packetData.BodyData);
            var room = GetRoom(reqData.RoomNubmer);

            if (room == null)
            {
                ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                return;
            }

            if(room.AddUser(user.ID(), sessionID) == false)
            {
                ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_FAIL_ADD_USER, sessionID);
                return;
            }

            user.RoomEnter(reqData.RoomNubmer);

            room.NotifyPacketUserList(sessionID);
            room.NotifyPacketNewUser(sessionID, user.ID());//방의 다른 유저에게 나 왔다고 알리기

            ResponseEnterRoomToClient(ERROR_CODE.NONE, sessionID);

            ServerNetwork.MainLogger.Debug("RequestEnterInternal - Success");
        }
        catch (Exception ex)
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    void ResponseEnterRoomToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resRoomEnter = new PKTResRoomEnter() 
        {
            Result = (Int16)errorCode
        };

        var body = MemoryPackSerializer.Serialize(resRoomEnter);
        var sendData = PacketMaker.MakePacket(PACKETID.RES_ROOM_ENTER, body);

        ServerNetwork.SendData(sessionID, sendData);
    }

    void RequestRoomLeave(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug("방 나가기 요청");

        try
        {
            var user = UserMgr.GetUser(sessionID);

            if (user == null)
            {
                ServerNetwork.MainLogger.Debug("유저 존재하지 않음");
                return;
                
            }

            if(RemoveUserFromRoom(sessionID, user.RoomNumber) == false)
            {
                ServerNetwork.MainLogger.Debug("올바른 방 나가기 요청이 아님");
                return ;
            }

            user.LeaveRoom();

            ResponseLeaveRoomToClient(sessionID);
            

            ServerNetwork.MainLogger.Debug("Request Leave Room - Success");

                                         
        }
        catch(Exception ex)
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    bool RemoveUserFromRoom(string sessionID, int roomNumber)
    {
        ServerNetwork.MainLogger.Debug($"LeaveRoomUser. SessionID: {sessionID}");

        var room = GetRoom(roomNumber);
        if (room == null)
        {
            return false;
        }

        var roomUser = room.GetUserByNetSessionID(sessionID);
        if (roomUser == null)
        {
            return false;
        }

        var userID = roomUser.UserID;
        room.RemoveUser(roomUser);

        room.NotifyPacketLeaveUser(userID);
        return true;
    }

    void ResponseLeaveRoomToClient(string sessionID)
    {
        var resRoomLeave = new PKTResRoomLeave()
        {
            Result = (Int16)ERROR_CODE.NONE
        };

        var body = MemoryPackSerializer.Serialize(resRoomLeave);
        var sendData = PacketMaker.MakePacket(PACKETID.RES_ROOM_LEAVE, body);
            
        ServerNetwork.SendData(sessionID, sendData);
    }

    public void NotifyLeaveInternal(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug($"NotifyLeaveInternal. SessionID: {sessionID}");

        var reqData = MemoryPackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.BodyData);
        RemoveUserFromRoom(sessionID, reqData.RoomNumber);
    }

    public void RequestChat(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug("Room Request Chat");

        try
        {
            var roomObject = CheckRoomAndRoomUser(sessionID);

            if(roomObject.Item1 == false)
            {
                return;
            }
            
            var reqData = MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packetData.BodyData);

            var notifyPacket = new PKTNtfRoomChat()
            {
                UserID = roomObject.Item3.UserID,
                ChatMessage = reqData.ChatMessage
            };

            var body = MemoryPackSerializer.Serialize(notifyPacket);
            var sendData = PacketMaker.MakePacket(PACKETID.NTF_ROOM_CHAT, body);

            roomObject.Item2.Broadcast("", sendData);
            //채팅 전달. 자기 자신에게도 전달됨
            //1st arg가 sessionID인데, 이것이 같으면 전달이 안됨. 근데 여기서는 ""로 했으므로
            //무조건 방 안 모든 유저에게 전송될 것(sessionID가 ""인 클라는 존재 X)

            ServerNetwork.MainLogger.Debug("Room Request Chat - Success");
        }
        catch (Exception ex)
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
    {
        var user = UserMgr.GetUser(userNetSessionID);

        if (user == null)
        {
            return (false, null, null);
        }

        var roomNumber = user.RoomNumber;
        var room = GetRoom(roomNumber);

        if (room == null)
        {
            return (false, null, null);
        }

        var roomUser = room.GetUserByNetSessionID(userNetSessionID);

        if (roomUser == null)
        {
            return (false, null, null);
        }

        return (true, room, roomUser);
    }

    public void NotifyToAllRoomUsers(string sessionID, string notifyBody)
    {
        var roomObject = CheckRoomAndRoomUser(sessionID);

        if (roomObject.Item1 == false)
        {
            return;
        }


        var notifyPacket = new PKTNtfRoomChat()
        {
            UserID = roomObject.Item3.UserID,
            ChatMessage = notifyBody
        };

        var body = MemoryPackSerializer.Serialize(notifyPacket);
        var sendData = PacketMaker.MakePacket(PACKETID.NTF_ROOM_CHAT, body);

        roomObject.Item2.Broadcast("", sendData);
    }
}
