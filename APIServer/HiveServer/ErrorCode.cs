using System;

//1000~19999 번대 사용하는 이유?
//오류코드를 너무 세밀하게 정리하면 클라이언트에게 디테일한 메시지 전달할 수 있지만 범용성이 낮아짐.
//오류코드를 너무 단순하게 정리하면 범용성이 좋아 여러 곳에서 사용할 수 있지만, 메시지를 세밀하게 작성하기 어려워짐.
public enum ErrorCode: UInt16
{
    None=0,

    //Common Error 처리: 1000~
    UnhandleException = 1001,
    RedisFailException=1002,
    InValidRequestHttpBody=1003,
    AuthTokenFailWrongAuthToken=1006,

    //Account 관련 Error 처리 2000~
    CreateAccountFailException=2001,
    CreateAccountDuplicatedUser=2020,
    FindAccountExistException = 2018,
    AccountAlreadyExist = 2019,
    //로그인 관련 Error
    LoginFailException =2002,
    LoginFailUserNotExist=2003,
    LoginFailWrongPassword=2004,
    LoginFailSetAuthToken=2006,//???
    LoginFailSetRecentDate=2022,
    DuplicatedLogin=2007,
    //인증토큰 관련 에러
    AuthTokenMismatch=2008,
    AuthTokenNotFound=2009,
    AuthTokenFailWrongKeyword=2010,
    //이 이후는 용도 알아봐야 함
    AuthTokenFailSetNx=2011,//???
    LoginFailAddRedis=2012,
    CheckAuthFailNotExist=2015,
    CheckAuthFailEmailNotMatch=2016,
    CheckAuthFailAuthTokenNotMatch = 2021,
    CheckAuthFailException =2017,

    //GameDB 4000~
    GetGameDBConnectionFail=4002
}