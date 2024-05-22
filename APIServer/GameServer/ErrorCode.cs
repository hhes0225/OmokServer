public enum ErrorCode : UInt16
{
    None=0,

    RedisFailException = 1002,

    //Account 관련 Error 처리 2000~
    CreateAccountFailException = 2001,
    CreateAccountDuplicatedUser = 2020,
    FindAccountExistException = 2018,
    AccountAlreadyExist = 2019,
    //로그인 관련 Error
    LoginFailException = 2002,
    LoginFailUserNotExist = 2003,
    LoginFailWrongPassword = 2004,
    LoginFailSetAuthToken = 2006,//???
    DuplicatedLogin = 2007,
    //인증토큰 관련 에러
    AuthTokenMismatch = 2008,
    AuthTokenNotFound = 2009,
    AuthTokenFailWrongKeyword = 2010,
    //이 이후는 용도 알아봐야 함
    AuthTokenFailSetNx = 2011,//???
    LoginFailAddRedis = 2012,
    CheckAuthFailNotExist = 2015,
    CheckAuthFailNotMatch = 2016,
    CheckAuthFailException = 2017,

    //User DB 관련 Error 5000~
    LoginFailHiveConnectionException =5001,
    UserDBInsertException=5002,

    //Matching 관련 Error 6000~
    MatchingFailError=6002,
    MatchingNotYet = 6003,

}