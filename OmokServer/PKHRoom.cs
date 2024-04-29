using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;
using MemoryPack;

namespace ChatServer;

//방에서 발생한 요청 처리
public class PKHRoom:PKHandler
{
    List<Room> RoomList = null;
    int StartRoomNumber;

    public void SetRoomList(List<Room> roomList)
    {
        RoomList = roomList;
        StartRoomNumber = roomList[0].Number;
    }

    public void RegisterPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.REQ_ROOM_ENTER, RequestRoomEnter);
    }
    
    //roomNumber에 맞는 Room 형 변수 리턴 -> 참조변수..?
    Room GetRoom(int roomNumber)
    {
        var index = roomNumber-StartRoomNumber;
        if (index < 0||index>=RoomList.Count()) 
        {
            return null;    
        }

        return RoomList[index];
    }

    (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
    {
        //UserMgr은 PKHandler에 지정되어 있음. 이걸 상속받았으므로 PKHRoom도 사용가능
        var user=UserMgr.GetUser(userNetSessionID);
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

        var roomUser= room.GetUserByNetSessionID(userNetSessionID);

        if (roomUser == null)
        {
            return (false, null, null);
        }
        
        return (true, room, roomUser);
    }

    public void RequestRoomEnter(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug("Request Room Enter");

        try
        {
            //유저 상태에 따라 발생할 수 있는 사항 처리

            var user = UserMgr.GetUser(sessionID);

            //유저 정보가 없으면
            if (user == null || user.IsConfirm(sessionID)) 
            {
                ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                return ;
            }

            //유저가 방에 들어갈 수 있는 상태가 아님
            if (user.IsStateRoom())
            {
                ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                return;
            }

            //이제 방 상태에 의해 발생할 수 있는 사항 처리
            
            //binary require data를 클래스 형식으로 변경
            var reqData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packetData.BodyData);
            var room = GetRoom(reqData.RoomNumber);

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

            user.EnteredRoom(reqData.RoomNumber);

            room.NotifyPacketUserList(sessionID);
            room.NotifyPacketNewUser(sessionID, user.ID());

            ResponseEnterRoomToClient(ERROR_CODE.NONE, sessionID);

            MainServer.MainLogger.Debug("RequestEnterInternal - Success");
        }
        catch (Exception ex)
        {
            MainServer.MainLogger.Error(ex.ToString());
        }
    }

    void ResponseEnterRoomToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resRoomEnter = new PKTResRoomEnter()
        {
            Result = (short)errorCode
        };

        var bodyData= MemoryPackSerializer.Serialize(resRoomEnter);
        var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_ENTER, bodyData);

        ServerNetwork.SendData(sessionID, sendData);
    }

    void ReqestLeave(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug("로그인 요청 받음");

        try
        {
            var user = UserMgr.GetUser(sessionID);

            if (user==null)
            {
                return;
            }

            if(LeaveRoomUser(sessionID, user.RoomNumber)==false)
            {
                return;
            }

            //위 사항들 통과하면 방을 나간다
            user.LeaveRoom();

            ResponseLeaveRoomToClient(sessionID);

            MainServer.MainLogger.Debug("Request Leave Room - Success");
        }
        catch(Exception ex)
        {
            MainServer.MainLogger.Error(ex.ToString());
        }
    }

    bool LeaveRoomUser(string sessionID, int roomNumber)
    {
        MainServer.MainLogger.Debug($"LeaveRoomUser. SessionID: {sessionID}");

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
            Result = (short)ERROR_CODE.NONE
        };

        var bodyData=MemoryPackSerializer.Serialize(resRoomLeave);
        var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_LEAVE, bodyData);

        ServerNetwork.SendData(sessionID, sendData);
    }

    public void NotifyLeaveInternal(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug($"NotifyLeaveInternal. SessionID: {sessionID}");

        var reqData = MemoryPackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.BodyData);
        LeaveRoomUser(sessionID, reqData.RoomNumber);
    }

    public void RequestChat(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug("Room Request Chat");

        try
        {
            var roomObject = CheckRoomAndRoomUser(sessionID);

            if (roomObject.Item1 == false)
            {
                return;
            }

            var reqData=MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packetData.BodyData);

            var notifyPacket = new PKTNtfRoomChat()
            {
                UserID = roomObject.Item3.UserID,
                ChatMessage = reqData.ChatMessage,
            };

            var Body = MemoryPackSerializer.Serialize(notifyPacket);
            var sendData = PacketToBytes.Make(PACKETID.NTF_ROOM_CHAT, Body);

            roomObject.Item2.Broadcast("", sendData);//채팅 전달하는 함수
            //Item2는 Room형 클래스 변수. Room 클래스에 Broadcast라는 메소드 구현 필요

            MainServer.MainLogger.Debug("Room Request Chat - Success");
        }
        catch(Exception ex) {
            MainServer.MainLogger.Error(ex.ToString());
        }
    }
 }
