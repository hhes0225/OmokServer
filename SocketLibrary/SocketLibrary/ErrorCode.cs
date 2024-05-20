using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;

namespace SocketLibrary;

//클라이언트와 정확히 동일한 내용이어야 함.
//클라이언트가 1001번 에러를 보냈을 때 서버에 에러 코드가 없어서 처리를 못해주거나 다른 에러 처리로 넘어갈수도 있음
public enum ErrorCode : short
{
    None = 0, // 정상처리, 에러가 아님

    //서버 초기화 에러(Redis)
    RedisInitFail = 1,

    //로그인 : 1000번대
    LoginInvalidAuthToken = 1001,
    AddUserDuplication = 1002,
    RemoveUserSearchFailureUserId = 1003,
    UserAuthSearchFailureUserId = 1004,
    UserAuthAlreadySetAuth = 1005,

    LoginAlreadyWorking = 1006,
    LoginFullUserCount = 1007,


    DbLoginInvalidPassword = 1011,
    DbLoginEmptyUser = 1012,
    DbLoginException = 1013,
    DbGameResultUpdateFail = 1014,
    DbAlreadyExistUser = 1015,

    RoomEnterInvalidState = 1021,
    RoomEnterInvalidUser = 1022,
    RoomEnterErrorSystem = 1023,
    RoomEnterInvalidRoomNumber = 1024,
    RoomEnterFailAddUser = 1025,

    OmokOverflow = 1031,
    OmokAlreadyExist = 1032,
    OmokRenjuRule = 1033, // 쌍삼
    OmokTurnNotMatch = 1034,
    OmokNotStarted = 1035,

    //Heartbeat
    HbUserNotExist = 1050,
}
