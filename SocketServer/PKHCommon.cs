using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

//방 기능과 관련없는 공통적인 로적 처리

public class PKHCommon : PKHandler
{
    PacketToBytes PacketMaker = new PacketToBytes();

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.NTF_IN_CONNECT_CLIENT, NotifyInConnectClient);
        packetHandlerMap.Add((int)PACKETID.NTF_IN_DISCONNECT_CLIENT, NotifyInDisconnectClient);
        packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);

    }

    public void NotifyInConnectClient(PacketData packetData)
    {
        ServerNetwork.MainLogger.Debug($"Current Connected Session Count: {ServerNetwork.SessionCount}");
    }

    public void NotifyInDisconnectClient(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var user = UserMgr.GetUser(sessionID);

        if(user != null)
        {
            //연결 끊길 시, 속해있던 방에서 나가기
            var roomNum = user.RoomNumber;

            if (roomNum != PacketDef.InvalidRoomNumber)
            {
                var packet = new PKTInternalNtfRoomLeave()
                {
                    UserID = user.ID(),
                    RoomNumber = roomNum
                };

                var body = MemoryPackSerializer.Serialize(packet);
                var internalPacket = new PacketData();
                internalPacket.Assign(sessionID, (Int16)PACKETID.NTF_IN_ROOM_LEAVE, body);

                ServerNetwork.Distribute(internalPacket);
            }
            


            // 유저 리스트에서 제거
            UserMgr.RemoveUser(sessionID);
        }

        ServerNetwork.MainLogger.Debug($"Current Connected Session Count: {ServerNetwork.SessionCount}");
    }

    // 클라이언트에게 로그인 요청 packet ID를 받으면 이 함수가 호출됨
    public void RequestLogin(PacketData packetData)
    {
        //session ID(오직 유저별로 1개의 통신 세션만 생성 가능)
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug("로그인 요청 받음");

        try
        {
            //session ID가 이미 존재한다면 이미 로그인 상태인 것임
            if(UserMgr.GetUser(sessionID) != null)
            {
                //에러 메시지와 함께 response 메시지 전달
                ResponseLoginToClient(ERROR_CODE.LOGIN_ALREADY_WORKING, sessionID);
                ServerNetwork.MainLogger.Debug("이미 로그인 중임");
                return;
            }

            //body deserialize & processor의 buffer에 삽입
            var reqData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packetData.BodyData);
            var errorCode = UserMgr.AddUser(reqData.UserID, sessionID);//유저 리스트에 유저 추가

            //packet생성해서 그 결과를 response
            if(errorCode == ERROR_CODE.NONE)
            {
                ServerNetwork.MainLogger.Debug($"{reqData.UserID} 로그인 성공");
                ResponseLoginToClient(errorCode, sessionID);
                //리스폰스 메시지 전달
                ServerNetwork.MainLogger.Debug("로그인 요청 답변 보냄");
            }
        }
        catch (Exception ex)
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    //Request 함수에서 response도 전송하고 있기 때문에 핸들러 등록 X
    public void ResponseLoginToClient(ERROR_CODE errorCode, string sessionID) 
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        //body 직렬화 + 헤더 직렬화(바디 헤더 따로따로 직렬화) 추가해서 전달
        //MakePacket에서 알아서 패킷 크기 계산해줌.
        var bodyData = MemoryPackSerializer.Serialize(resLogin);
        var sendData = PacketMaker.MakePacket(PACKETID.RES_LOGIN, bodyData);

        ServerNetwork.SendData(sessionID, sendData);//패킷 버퍼에 삽입
    }

    public void NotifyMustCloseToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resLogin = new PKTNtfMustClose()
        {
            Result = (short)errorCode
        };

        var bodyData = MemoryPackSerializer.Serialize(resLogin);
        var sendData = PacketMaker.MakePacket(PACKETID.NTF_MUST_CLOSE, bodyData);

        ServerNetwork.SendData(sessionID , sendData);
    }
}

    
