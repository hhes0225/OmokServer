using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;

namespace SocketLibrary;


//각 req, res 유형에 따라 필요한 class 정의
//(serialize, deserialize 하기 위함)
//따라서 client측에서 보내주는 req도 정의 필요
//body와 header 따로 만들어주기 때문에 body 클래스만 정의

[MemoryPackable]
public partial class PKTReqLogin//cli->srv 로그인 reqest 패킷
{
    public string UserID {  get; set; }
    public string AuthToken {  get; set; }
    public string SessionID { get; set; }
}

[MemoryPackable]
public partial class PKTResLogin//srv->cli 로그인 결과 response
{
    public short Result {  get; set; }
}

[MemoryPackable]
public partial class PKTNtfMustClose//Notify-로그인 실패한 경우 반드시 close해야 함을 명시..?
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomEnter//cli->srv 방 입장 request
{
    public int RoomNubmer {  get; set; }
}

[MemoryPackable]
public partial class PKTResRoomEnter//srv->cli 방 입장 결과 response
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomUserList//방 입장 시 방 안 유저에게 서로의 존재를 notify?
{
    public List<string> UserIDList { get; set; } = new List<string>();
}

[MemoryPackable]
public partial class PKTNtfRoomNewUser//방에 새 유저 입장 시 이미 방에 있는 사람들에게 Notify
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomLeave//cli->srv 방 퇴장 request
{
}

[MemoryPackable]
public partial class PKTResRoomLeave//srv->cli 방 퇴장 결과 response
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomLeaveUser//방 구성원에게 나간 유저 정보 Notify
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomChat//cli->srv 채팅 메시지 보내기 request
{
    public string ChatMessage {  get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomChat//Ntf == Response 역할을 하게 됨. 실행 결과 채팅 메시지 다른 사용자들에게 broadcast.
{
    public string UserID { get; set; }
    public string ChatMessage { get; set; }
}

[MemoryPackable]
public partial class PKTReqReadyOmok
{
    public int RoomNumber;
    public string UserID;
}

[MemoryPackable]
public partial class PKTResReadyOmok
{
    public short Result;
}

[MemoryPackable]
public partial class PKTNtfReadyOmok
{
    public string UserID;
    public short IsReady;
}

// 오목 시작 통보(서버에서 클라이언트들에게)
[MemoryPackable]
public partial class PKTNtfStartOmok
{
    public string BlackUserID; // 선턴 유저 ID
    public string WhiteUserID;
}

// 돌 두기
[MemoryPackable]
public partial class PKTReqPutMok
{
    public int RoomNumber;
    public int PosX;
    public int PosY;
}

[MemoryPackable]
public partial class PKTResPutMok
{
    public short Result;
}

[MemoryPackable]
public partial class PKTNtfPutMok
{
    public int PosX;
    public int PosY;
    public int Mok;
}

[MemoryPackable]
public partial class PKTNtfTurnPass
{
}

// 오목 게임 종료 통보
[MemoryPackable]
public partial class PKTNtfEndOmok
{
    public string WinUserID;
}

//하트비트 - 유저 연결 상태 확인용(핑퐁)
[MemoryPackable]
public partial class PTKPingUserConnInfo
{
}

[MemoryPackable]
public partial class PKTPongUserConnINfo
{
    public Int16 Result;
}


[MemoryPackable]
public partial class PKTReqDBLogin
{
    public string Id;
    public string AuthToken;
}

[MemoryPackable]
public partial class PKTResDBLogin
{
    public short Result;
}


