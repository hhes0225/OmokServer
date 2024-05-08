using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class PKHHeartbeat:PKHandler
{
    PacketToBytes PacketMaker = new PacketToBytes();
    private int _startIndexUserCheck = 0;
    private const int MaxCheckUserCount = 250;

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.PING_USER_CONN_INFO, PingUserConnInfo);
        packetHandlerMap.Add((int)PACKETID.NTF_INNER_USER_CHECK, NotifyInnerUserCheck);
    }

    public void PingUserConnInfo(PacketData packetData)
    {
        var sessionID = packetData.SessionID;

        try
        {
            //User 정보 업데이트
            //유저 찾기
            var user = UserMgr.GetUserByNetSessionID(sessionID);

            //유저 정보 없음
            if (user == null)
            {
                PongUserConnInfo(ERROR_CODE.HB_USER_NOT_EXIST, sessionID);
                return;
            }

            //유저 정보 있음
            user.UpdateHeartbeat(DateTime.Now);

            PongUserConnInfo(ERROR_CODE.NONE, sessionID);
        }
        catch(Exception ex)
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    public void PongUserConnInfo(ERROR_CODE errorCode, string sessionID)
    {
        var pongUserConnInfo = new PKTPongUserConnINfo
        {
            Result = (Int16)errorCode
        };

        var body = MemoryPackSerializer.Serialize(pongUserConnInfo);
        var sendData = PacketMaker.MakePacket(PACKETID.PONG_USER_CONN_INFO, body);

        ServerNetwork.SendData(sessionID, sendData);
    }

    public void NotifyInnerUserCheck(PacketData packetData)
    {
        var endIndex = _startIndexUserCheck + MaxCheckUserCount;
        UserMgr.CheckHeartBeat(_startIndexUserCheck, endIndex);
        UserMgr.DisconnectInactiveUser(_startIndexUserCheck, endIndex);

        _startIndexUserCheck += MaxCheckUserCount;

        if (_startIndexUserCheck >= UserMgr.GetMaxUserCount()) {
            _startIndexUserCheck = 0;
        }
    }
}
