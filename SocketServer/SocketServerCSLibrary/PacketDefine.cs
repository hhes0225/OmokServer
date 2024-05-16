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
    None=0, // 정상처리, 에러가 아님

    //서버 초기화 에러(Redis)
    RedisInitFail=1,

    //로그인 : 1000번대
    LoginInvalidAuthToken = 1001,
    AddUserDuplication = 1002,
    RemoveUserSearchFailureUserId=1003,
    UserAuthSearchFailureUserId=1004,
    UserAuthAlreadySetAuth=1005,
    
    LoginAlreadyWorking=1006,
    LoginFullUserCount=1007,
    

    DbLoginInvalidPassword=1011,
    DbLoginEmptyUser=1012,
    DbLoginException=1013,
    DbGameResultUpdatFail = 1014,

    RoomEnterInvalidState=1021,
    RoomEnterInvalidUser=1022,
    RoomEnterErrorSystem=1023,
    RoomEnterInvalidRoomNumber=1024,
    RoomEnterFailAddUser=1025,

    OmokOverflow = 1031,
    OmokAlreadyExist = 1032,
    OmokRenjuRule = 1033, // 쌍삼
    OmokTurnNotMatch = 1034,
    OmokNotStarted = 1035,

    //Heartbeat
    HbUserNotExist = 1050,
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
    ReqResTestEcho=101,

    //클라이언트
    CsBegin=1001,

    ReqLogin=1002,
    ResLogin=1003,
    NtfMustClose=1005,

    ReqRoomEnter=1015,
    ResRoomEnter=1016,
    NtfRoomUserList=1017,
    NtfRoomNewUser=1018,

    ReqRoomLeave=1021,
    ResRoomLeave=1022,
    NtfRoomLeaveUser=1023,

    ReqRoomChat=1026,
    NtfRoomChat=1028,

    ReqReadyOmok=1031,
    ResReadyOmok=1032,
    NtfReadyOmok=1033,

    NtfStartOmok = 1034,

    ReqPutOmok = 1035,
    ResPutOmok = 1036,
    NtfPutOmok = 1037,

    NtfEndOmok = 1038,

    //heartbeat
    PingUserConnInfo = 1039,
    PongUserConnInfo = 1040,

    NtfTurnPass = 1041,
    

    ReqRoomDevAllRoomStartGame = 1091,
    ResRoomDevAllRoomStartGame=1092,

    ReqRoomDevAllRoomEndGame=1093,
    ResRoomDevAllRoomEndGame=1094,

    CsEnd=1100,


    //시스템, 서버 - 서버
    SsStart = 8001,

    NtfInConnectClient=8011,
    NtfInDisconnectClient=8012,
    NtfInnerUserCheck = 8013,
    NtfInnerRoomCheck = 8014,
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
    NtfInInsertTestUser=8104
}