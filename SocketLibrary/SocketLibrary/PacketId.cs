using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLibrary;



//packet id = 요청 타입(패킷이 서버에게 어떤 기능을 해주기를 요청하는가)
//1 ~ 10000번대 사용

public enum PACKETID : int
{
    ReqResTestEcho=101,

    //from client, request: 1000번대

    ReqBegin=1000,

    ReqLogin=1001,
    ReqRoomEnter = 1002,
    ReqRoomLeave = 1003,
    ReqRoomChat = 1004,
    ReqReadyOmok = 1005,
    ReqPutOmok = 1006,
    PingUserConnInfo = 1007,
    ReqRoomDevAllRoomStartGame = 1008,
    ReqRoomDevAllRoomEndGame = 1009,

    ReqEnd = 1999,

    //to client, response: 2000번대
    ResBegin = 2000,

    ResLogin = 2001,
    ResRoomEnter = 2002,
    ResRoomLeave = 2003,
    ResReadyOmok = 2004,
    ResPutOmok = 2005,
    PongUserConnInfo = 2006,
    ResRoomDevAllRoomStartGame = 2007,
    ResRoomDevAllRoomEndGame = 2008,
    ResRoomChat = 2009,

    ResEnd = 2999,


    //to client, notify: 3000번대
    NtfBegin = 3000,

    NtfMustClose = 3001,
    NtfRoomUserList = 3002,
    NtfRoomNewUser = 3003,
    NtfRoomLeaveUser = 3004,
    NtfRoomChat = 3005,
    NtfReadyOmok = 3006,
    NtfStartOmok = 3007,
    NtfPutOmok = 3008,
    NtfEndOmok = 3009,
    NtfTurnPass = 3010,

    NtfEnd = 3999,



    //시스템, 서버 - 서버
    SsStart = 8001,

    NtfInConnectClient=8011,
    NtfInDisconnectClient=8012,
    NtfInUserCheck = 8013,
    NtfInRoomCheck = 8014,
    NtfInnerTurnCheck=8015,

    ReqSsServerInfo =8021,
    ResSsServerInfo=8023,

    ReqInRoomEnter=8031,
    ResInRoomLeave=8032,

    NtfInRoomLeave=8036,

    
    //DB 8101~9000
    ReqDbLogin=8101,
    ResDbLogin=8102,
    NtfInGameResultUpdate=8103,
    ReqInInsertTestUser=8104,
    ResInInsertTestUser=8105,
}
