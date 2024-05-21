using SocketLibrary;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PacketHandler;

//방 기능과 관련없는 공통적인 로적 처리

public class PKHCommon : PKHandler
{
    PacketToBytes PacketMaker = new PacketToBytes();
    int SessionCount = 0;

    public static Action<PacketData> RedisDistributeFunc;

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.NtfInConnectClient, NotifyInConnectClient);
        packetHandlerMap.Add((int)PACKETID.NtfInDisconnectClient, NotifyInDisconnectClient);
        packetHandlerMap.Add((int)PACKETID.ReqLogin, RequestLogin);

    }

    public void NotifyInConnectClient(PacketData packetData)
    {
        SessionCount++;
        HandlerLogger.Debug($"Current Connected Session Count: {SessionCount}");
        var sessionID = packetData.SessionID;

        _userMgr.AddUser("", sessionID);
    }

    public void NotifyInDisconnectClient(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var user = _userMgr.GetUserBySessionID(sessionID);

        if (user != null)
        {
            var roomNum = user.RoomNumber;

            if (roomNum != PacketDef.InvalidRoomNumber)
            {
                var packet = new PKTInternalNtfRoomLeave()
                {
                    UserID = user.UserSessionID(),
                    RoomNumber = roomNum
                };

                var body = MemoryPackSerializer.Serialize(packet);
                var internalPacket = new PacketData();
                internalPacket.Assign(sessionID, (short)PACKETID.NtfInRoomLeave, body);

                DistributeFunc(internalPacket);
            }

            _userMgr.RemoveUser(sessionID);
        }

        HandlerLogger.Debug($"Current Connected Session Count: {--SessionCount}");
    }

    public void RequestLogin(PacketData packetData)
    {
        var reqData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packetData.BodyData);
        var sessionID = reqData.SessionID;

        HandlerLogger.Debug("로그인 요청 받음");

        try
        {
            if (_userMgr.GetUserBySessionID(sessionID).ID() != "")
            {
                ResponseLoginToClient(ErrorCode.LoginAlreadyWorking, sessionID);
                HandlerLogger.Debug("이미 로그인 중임");
                return;
            }

            //body deserialize & processor의 buffer에 삽입
            //접속할 때 이미 추가된 user 세션을 찾아서 userID만 갱신
            var user = _userMgr.GetUserBySessionID(sessionID);
            var errorCode = user.LoginUpdateID(reqData.UserID);

            if (errorCode == ErrorCode.None)
            {
                HandlerLogger.Debug($"{reqData.UserID} 로그인 성공");
                ResponseLoginToClient(errorCode, sessionID);
               
                HandlerLogger.Debug("로그인 요청 답변 보냄");
            }
        }
        catch (Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }
    }

    public void ResponseLoginToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var bodyData = MemoryPackSerializer.Serialize(resLogin);
        var sendData = PacketMaker.MakePacket(PACKETID.ResLogin, bodyData);

        SendDataFunc(sessionID, sendData);
    }

    public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKTNtfMustClose()
        {
            Result = (short)errorCode
        };

        var bodyData = MemoryPackSerializer.Serialize(resLogin);
        var sendData = PacketMaker.MakePacket(PACKETID.NtfMustClose, bodyData);

        SendDataFunc(sessionID, sendData);
    }
}


