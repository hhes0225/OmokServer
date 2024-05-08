using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSBaseLib;

public enum UserState
{
    None = 0,
    Ready = 1,
    Playing = 2,
    Finished = 3
}

//클라이언트와 정확히 동일한 내용이어야 함.
//클라이언트가 1001번 에러를 보냈을 때 서버에 에러 코드가 없어서 처리를 못해주거나 다른 에러 처리로 넘어갈수도 있음

public enum ERROR_CODE : short
{
    NONE=0, // 정상처리, 에러가 아님

    //서버 초기화 에러(Redis)
    REDIS_INIT_FAIL=1,

    //로그인 : 1000번대
    LOGIN_INVALID_AUTHTOKEN = 1001,
    ADD_USER_DUPLICATION = 1002,
    REMOVE_USER_SEARCH_FAILURE_USER_ID=1003,
    USER_AUTH_SEARCH_FAILURE_USER_ID=1004,
    USER_AUTH_ALREADY_SET_AUTH=1005,
    
    LOGIN_ALREADY_WORKING=1006,
    LOGIN_FULL_USER_COUNT=1007,
    

    DB_LOGIN_INVALID_PASSWORD=1011,
    DB_LOGIN_EMPTY_USER=1012,
    DB_LOGIN_EXCEPTION=1013,

    ROOM_ENTER_INVALID_STATE=1021,
    ROOM_ENTER_INVALID_USER=1022,
    ROOM_ENTER_ERROR_SYSTEM=1023,
    ROOM_ENTER_INVALID_ROOM_NUMBER=1024,
    ROOM_ENTER_FAIL_ADD_USER=1025,

    OMOK_OVERFLOW = 1031,
    OMOK_ALREADY_EXIST = 1032,
    OMOK_RENJURULE = 1033, // 쌍삼
    OMOK_TURN_NOT_MATCH = 1034,
    OMOK_NOT_STARTED = 1035,

    //Heartbeat
    HB_USER_NOT_EXIST = 1050,
}


//packet id = 요청 타입(패킷이 서버에게 어떤 기능을 해주기를 요청하는가)
//1 ~ 10000번대 사용

//1. mainServer.cs onConnected, onClosed, onPacketReceived로 이벤트(연결, 연결해제, 패킷받기) 감지
//  *** 이때, 패킷 해석 결과 (ReceiveFilter.cs ResolveRequestInfo 메서드)는 onPacketReceived에서 호출된다
//2. 위 이벤트 함수들에서 mainServer.cs Distribute()로 패킷 분석 결과를
//  스레드(MainPacketProcessor-PacketProcessor.cs)에 넣는다(message buffer)
//3. PacketProcess 클래스에는 여러 패킷 핸들러(PKHandler)가 있다. 패킷 헤더에서 PacketID를 뽑아와서
//      PacketID 동작에 맞는 함수를 등록한다(PacketID: PacketDefine.cs)
//      등록: PacketProcessor.cs->RegisterPacketHandler(여기서 PKHCommon, PKHRooms 패킷 핸들러 등록)
//      (PacketHandleMap 딕셔너리에 등록 -> PKHCommon, PKHRooms에서 설정한 대로 자동으로 등록해줌)

//      이때, 함수에는 함수 동작에 맞게 바디를 클래스화시켜주고 있다.

//4. 등록한 함수는 언제 호출될까? MainServer에서 스레드를 돌릴 때
//   PacketProcess.cs -> Process()에서 호출된다.
//  이 부분은 2단계에서 버퍼에 넣은 패킷(주문서)를 읽어서 일꾼이 (주문서대로) 일을 처리하는 부분이다.
//   var packet = MsgBuffer.Receive(); 에서 패킷을 리시브하고,

//      if (PacketHandleMap.ContainsKey(packet.PacketID))
//                {
//                    PacketHandleMap[packet.PacketID] (packet);
//                }
//      이 패킷을 분석해서 딕셔너리에 맞는 패킷 핸들러 동작을 해준다.
//      
//      Receive가 되는 것은 버퍼에 값을 입력하는 Post가 들어올 때를 감지해서 Post가 들어오지 않으면 stop한다.
//      Receive는 버퍼가 빈 버퍼가 될 때까지 수행한다.

public enum PACKETID : int
{
    REQ_RES_TEST_ECHO=101,

    //클라이언트
    CS_BEGIN=1001,

    REQ_LOGIN=1002,
    RES_LOGIN=1003,
    NTF_MUST_CLOSE=1005,

    REQ_ROOM_ENTER=1015,
    RES_ROOM_ENTER=1016,
    NTF_ROOM_USER_LIST=1017,
    NTF_ROOM_NEW_USER=1018,

    REQ_ROOM_LEAVE=1021,
    RES_ROOM_LEAVE=1022,
    NTF_ROOM_LEAVE_USER=1023,

    REQ_ROOM_CHAT=1026,
    NTF_ROOM_CHAT=1028,

    REQ_READY_OMOK=1031,
    RES_READY_OMOK=1032,
    NTF_READY_OMOK=1033,

    NTF_START_OMOK = 1034,

    REQ_PUT_OMOK = 1035,
    RES_PUT_OMOK = 1036,
    NTF_PUT_OMOK = 1037,

    NTF_END_OMOK = 1038,

    //heartbeat
    PING_USER_CONN_INFO = 1039,
    PONG_USER_CONN_INFO = 1040,
    

    REQ_ROOM_DEV_ALL_ROOM_START_GAME = 1091,
    RES_ROOM_DEV_ALL_ROOM_START_GAME=1092,

    REQ_ROOM_DEV_ALL_ROOM_END_GAME=1093,
    RES_ROOM_DEV_ALL_ROOM_END_GAME=1094,

    CS_END=1100,


    //시스템, 서버 - 서버
    SS_START = 8001,

    NTF_IN_CONNECT_CLIENT=8011,
    NTF_IN_DISCONNECT_CLIENT=8012,
    NTF_INNER_USER_CHECK = 8013,
    NTF_INNER_ROOM_CHECK = 8014,

    REQ_SS_SERVERINFO =8021,
    RES_SS_SERVERINFO=8023,

    REQ_IN_ROOM_ENTER=8031,
    RES_IN_ROOM_LEAVE=8032,

    NTF_IN_ROOM_LEAVE=8036,

    
    //DB 8101~9000
    REQ_DB_LOGIN=8101,
    RES_DB_LOGIN=8102,



   
}