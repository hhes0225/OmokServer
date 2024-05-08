using MemoryPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSCommon
{
    public enum PutStoneResult
    {
        Success = 0,
        SamSam = 1,
    }

    public class UserState
    {
        public const Int16 None = 0;
        public const Int16 Ready = 1;
        public const Int16 Playing = 2;
        public const Int16 Finished = 3;
    }
    public class PacketDef
    {
        public const Int16 PacketHeaderSize = 4;
        public const int MaxUserIDByteLength = 16;
        public const int MaxUserPWByteLength = 16;
        public const int InvalidRoomNumber = -1;
    }

    // 로그인 요청
    [MemoryPackable]
    public partial class PKTReqLogin 
    {
        public string UserID;
        public string AuthToken;
    }

    [MemoryPackable]
    public partial class PKTResLogin 
    {
        public short Result;
    }



    [MemoryPackable]
    public partial class PKTNtfMustClose 
    {
        public short Result;
    }



    // 방 입장
    [MemoryPackable]
    public partial class PKTReqRoomEnter 
    {
        public int RoomNum;
    }

    [MemoryPackable]
    public partial class PKTResRoomEnter 
    {
        public short Result;
    }

    [MemoryPackable]
    public partial class PKTNtfRoomUserList 
    {
        public List<string> UserIDList = new List<string>();
    }

    [MemoryPackable]
    public partial class PKTNtfRoomNewUser 
    {
        public string UserID;
    }


    // 방 나가기(보디가 없다)
    [MemoryPackable]
    public partial class PKTReqRoomLeave 
    {
    }

    [MemoryPackable]
    public partial class PKTResRoomLeave 
    {
        public short Result;
    }

    [MemoryPackable]
    public partial class PKTNtfRoomLeaveUser 
    {
        public string UserID;
    }


    [MemoryPackable]
    public partial class PKTReqRoomChat 
    {
        public string ChatMessage;
    }

    [MemoryPackable]
    public partial class PKTResRoomChat 
    {
        public short Result;
    }

    [MemoryPackable]
    public partial class PKTNtfRoomChat 
    {
        public string UserID;
        public string ChatMessage;
    }

    [MemoryPackable]
    public partial class PKTInternalReqRoomEnter 
    {
        public int RoomNumber;
        public string UserID;
    }


    // 오목 플레이 준비 완료 요청
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




}
