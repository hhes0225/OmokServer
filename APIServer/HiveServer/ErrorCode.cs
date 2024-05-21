using System;

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
    LoginFailSetAuthToken=2006,
    LoginFailSetRecentDate=2022,
    DuplicatedLogin=2007,
    //인증토큰 관련 에러
    AuthTokenMismatch=2008,
    AuthTokenNotFound=2009,
    AuthTokenFailWrongKeyword=2010,

    AuthTokenFailSetNx=2011,
    LoginFailAddRedis=2012,
    CheckAuthFailNotExist=2015,
    CheckAuthFailEmailNotMatch=2016,
    CheckAuthFailAuthTokenNotMatch = 2021,
    CheckAuthFailException =2017,

    //GameDB 4000~
    GetGameDBConnectionFail=4002
}
