using SocketLibrary;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PacketHandler;

public class PKHHeartbeat : PKHandler
{
    PacketToBytes PacketMaker = new PacketToBytes();
    private int _startIndexUserCheck = 0;
    private const int MaxCheckUserCount = 250;

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.PingUserConnInfo, PingUserConnInfo);
        packetHandlerMap.Add((int)PACKETID.NtfInUserCheck, NotifyInUserCheck);
    }

    public void PingUserConnInfo(PacketData packetData)
    {
        var sessionID = packetData.SessionID;

        try
        {
            var user = _userMgr.GetUserBySessionID(sessionID);

            if (user == null)
            {
                PongUserConnInfo(ErrorCode.HbUserNotExist, sessionID);
                return;
            }

            user.UpdateHeartbeat(DateTime.Now);

            PongUserConnInfo(ErrorCode.None, sessionID);
        }
        catch (Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }
    }

    public void PongUserConnInfo(ErrorCode errorCode, string sessionID)
    {
        var pongUserConnInfo = new PKTPongUserConnINfo
        {
            Result = (short)errorCode
        };

        var body = MemoryPackSerializer.Serialize(pongUserConnInfo);
        var sendData = PacketMaker.MakePacket(PACKETID.PongUserConnInfo, body);

        SendDataFunc(sessionID, sendData);
    }

    public void NotifyInUserCheck(PacketData packetData)
    {
        var endIndex = _startIndexUserCheck + MaxCheckUserCount;
        _userMgr.CheckHeartBeat(_startIndexUserCheck, endIndex);
        //_userMgr.DisconnectInactiveUser(_startIndexUserCheck, endIndex);

        _startIndexUserCheck += MaxCheckUserCount;

        if (_startIndexUserCheck >= _userMgr.GetMaxUserCount())
        {
            _startIndexUserCheck = 0;
        }
    }
}
