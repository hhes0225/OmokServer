using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer;

//방 Room과 관계없는 공통적인 로직 처리
public class PKHCommon:PKHandler
{
    public void RegisterPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.NTF_IN_CONNECT_CLIENT, NotifyInConnectClient);
        packetHandlerMap.Add((int)PACKETID.NTF_IN_DISCONNECT_CLIENT, NotifyInDisconnectClient);
        packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);
    }

    public void NotifyInConnectClient(ServerPacketData packetData)
    {
        MainServer.MainLogger.Debug($"Current Connected Session Count:{ServerNetwork.SessionCount}");
    }

    public void NotifyInDisconnectClient(ServerPacketData requestData)
    {
        var sessionID = requestData.SessionID;
        var user=UserMgr.GetUser(sessionID);

        if(user != null)
        {
            var roomNum = user.RoomNumber;

            if(roomNum!= PacketDef.INVALID_ROOM_NUMBER)
            {
                var packet = new PKTInternalNtfRoomLeave()
                {
                    RoomNumber = roomNum,
                    UserID = user.ID()
                };

                var packetBodyData=MemoryPackSerializer.Serialize(packet);
                var internalPacket = new ServerPacketData();
                internalPacket.Assign(sessionID, (Int16)PACKETID.NTF_IN_ROOM_LEAVE, packetBodyData);

                ServerNetwork.Distribute(internalPacket);
            }

            UserMgr.RemoveUser(sessionID);
        }
        MainServer.MainLogger.Debug($"Current Connected Session Count: {ServerNetwork.SessionCount}");

    }


    public void RequestLogin(ServerPacketData packetData) 
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug("로그인 요청 받음");

        try
        {
            //sessionID는 고유한 것인데 이미 유저 관리에 있다면
            //유저 관리는 로그인한 유저만 관리하고 있음
            //->중복 로그인
            if(UserMgr.GetUser(sessionID) != null)
            {
                ResponseLoginToClient(ERROR_CODE.LOGIN_ALREADY_WORKING, packetData.SessionID);
                return;
            }

            //binary 데이터를 -> PKTReqLogin 클래스 형식으로
            var reqData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packetData.BodyData);
            var errorCode = UserMgr.AddUser(reqData.UserID, sessionID);
            if (errorCode != ERROR_CODE.NONE)
            {
                ResponseLoginToClient(errorCode, packetData.SessionID);
                MainServer.MainLogger.Debug("로그인 요청 답변 보냄");
            }
        }
        catch(Exception ex)
        {
            //패킷 해제에 의해 로그 남지 않도록 로그 수준 Debug 로 해야됨
            //로그 수준 정리????
            MainServer.MainLogger.Error(ex.ToString());
        }
    }

    //RequestLogin에서만 호출되고 있으므로 packetHandler에 등록하지 않는다.
    public void ResponseLoginToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var bodyData = MemoryPackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PACKETID.RES_LOGIN, bodyData);

        ServerNetwork.SendData(sessionID, sendData);
    }

    //왜 애는 핸들러에 등록되지 않는지?
    public void NotifyMustCloseToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var bodyData=MemoryPackSerializer.Serialize(resLogin);
        var sendData=PacketToBytes.Make(PACKETID.NTF_MUST_CLOSE, bodyData);

        ServerNetwork.SendData(sessionID, sendData);
    }
}
