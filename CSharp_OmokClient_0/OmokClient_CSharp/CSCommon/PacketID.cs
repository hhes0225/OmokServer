using System;
using System.Collections.Generic;
using System.Text;

namespace CSCommon;

// 1001 ~ 2000
public class PacketID
{
    public const UInt16 ReqResTestEcho = 101;

    //from client, request: 1000번대
    public const UInt16 ReqBegin = 1000;
    public const UInt16 ReqLogin = 1001;
    public const UInt16 ReqRoomEnter = 1002;
    public const UInt16 ReqRoomLeave = 1003;
    public const UInt16 ReqRoomChat = 1004;
    public const UInt16 ReqReadyOmok = 1005;
    public const UInt16 ReqPutMok = 1006;
    public const UInt16 PingUserConnInfo = 1007;
    public const UInt16 ReqRoomDevAllRoomStartGame = 1008;
    public const UInt16 ReqRoomDevAllRoomEndGame = 1009;
    public const UInt16 ReqEnd = 1999;

    //to client, response: 2000번대
    public const UInt16 ResBegin = 2000;
    public const UInt16 ResLogin = 2001;
    public const UInt16 ResRoomEnter = 2002;
    public const UInt16 ResRoomLeave = 2003;
    public const UInt16 ResReadyOmok = 2004;
    public const UInt16 ResPutMok = 2005;
    public const UInt16 PongUserConnInfo = 2006;
    public const UInt16 ResRoomDevAllRoomStartGame = 2007;
    public const UInt16 ResRoomDevAllRoomEndGame = 2008;
    public const UInt16 ResRoomChat = 2009;
    public const UInt16 ResEnd = 2999;

    //to client, notify: 3000번대
    public const UInt16 NtfBegin = 3000;
    public const UInt16 NtfMustClose = 3001;
    public const UInt16 NtfRoomUserList = 3002;
    public const UInt16 NtfRoomNewUser = 3003;
    public const UInt16 NtfRoomLeaveUser = 3004;
    public const UInt16 NtfRoomChat = 3005;
    public const UInt16 NtfReadyOmok = 3006;
    public const UInt16 NtfStartOmok = 3007;
    public const UInt16 NtfPutMok = 3008;
    public const UInt16 NtfEndOmok = 3009;
    public const UInt16 NtfTurnPass = 3010;
    public const UInt16 NtfEnd = 3999;

    //시스템, 서버 - 서버
    public const UInt16 SsStart = 8001;
    public const UInt16 NtfInConnectClient = 8011;
    public const UInt16 NtfInDisconnectClient = 8012;
    public const UInt16 NtfInnerUserCheck = 8013;
    public const UInt16 NtfInnerRoomCheck = 8014;
    public const UInt16 NtfInnerTurnCheck = 8015;
    public const UInt16 ReqSsServerInfo = 8021;
    public const UInt16 ResSsServerInfo = 8023;
    public const UInt16 ReqInRoomEnter = 8031;
    public const UInt16 ResInRoomLeave = 8032;
    public const UInt16 NtfInRoomLeave = 8036;

    //DB 8101~9000
    public const UInt16 ReqDbLogin = 8101;
    public const UInt16 ResDbLogin = 8102;
    public const UInt16 NtfInGameResultUpdate = 8103;
    public const UInt16 NtfInInsertTestUser = 8104;
}
