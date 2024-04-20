using System;
using System.Data;
using SqlKata.Execution; //QueryFactory 사용하기 위함(그냥 SqlKata는 안됨)
using HiveServer.Services;
using Microsoft.Extensions.Options;
using MySqlConnector;
using ZLogger;


namespace HiveServer.Repository;

public class AccountDB:IAccountDB
{
	private readonly IOptions<DBConfig> _dbConfig;
	private IDbConnection _dbConnection;
	private readonly SqlKata.Compilers.MySqlCompiler _compiler;
	private readonly QueryFactory _queryFactory;
	private readonly ILogger<AccountDB> _logger;


    public AccountDB(ILogger<AccountDB> logger, IOptions<DBConfig> dbConfig)
	{
		_dbConfig = dbConfig;
		_logger = logger;

		Open();//DB connect private 메소드

		_compiler = new SqlKata.Compilers.MySqlCompiler();
		_queryFactory = new QueryFactory(_dbConnection, _compiler);

	}
 
	//HiveDB 서버와 Disconnect
	public void Dispose()
	{
		Close();
	}

    //계정 생성 전, Hive DB에 해당 계정 존재 여부 조회
    public async Task<ErrorCode> FindAccountExistAsync(string email)
	{  
        try
        {
            long count = _queryFactory.Query("Account").Where("email", email).Count<dynamic>();

            if (count != 0)
            {
                return ErrorCode.AccountAlreadyExist;
            }
        }
        catch (Exception e)
        {
            return ErrorCode.FindAccountExistException;
        }

        return ErrorCode.None;
    }

    //계정 생성 시 DB에 사용자 추가
    public async Task<ErrorCode> CreateAccountAsync(string email, string pw)
	{
        try
        {
            string saltValue = Security.SaltString();
            string hashedPasword = Security.HashPassword(saltValue, pw);

            int count = await _queryFactory.Query("Account").InsertAsync(new dbAccountInfo
            {
                //uid는 auto increment로 자동 생성
                email = email,
                pw=hashedPasword,
                salt_value=saltValue,
                create_date= DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                recent_login_date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            });


            //Insert문은 삽입 성공 시 삽입 성공한 데이터 개수를 반환함.
            if (count != 1) {
                return ErrorCode.CreateAccountDuplicatedUser;
            }

        }
        catch(Exception e)
        {
            return ErrorCode.CreateAccountFailException;
        }

		return ErrorCode.None;//임의로 넣은 것
	}

	//로그인: 사용자 인증
    public async Task<Tuple<ErrorCode, String>> VerifyUser(String email, string pw)
	{
        string token;
        try
        {
            dbAccountInfo accountInfo = await _queryFactory.Query("Account").Where("email", email).FirstOrDefaultAsync<dbAccountInfo>();

            //계정 정보 찾을 수 없음
            if (accountInfo == null)
            {
                return new Tuple<ErrorCode, String>(ErrorCode.LoginFailUserNotExist, "0");
            }

            //request 암호화 비밀번호 - DB 암호화 비밀번호 비교
            string hashedPasword = Security.HashPassword(accountInfo.salt_value, pw);
            if(hashedPasword != accountInfo.pw)
            {
                return  new Tuple <ErrorCode, string> (ErrorCode.LoginFailWrongPassword, "0");
            }

        }
        catch (Exception e)
        {
            return new Tuple<ErrorCode, string>(ErrorCode.LoginFailException, "0");
        }

        return new Tuple<ErrorCode, string>(ErrorCode.None, email);
    }

    private void Open()
    {
        _dbConnection = new MySqlConnection(_dbConfig.Value.AccountDB);
        _dbConnection.Open();
    }


    private void Close()
	{
		_dbConnection.Close();
	}
}

public class DBConfig
{
    public string AccountDB { get; set; }
    public string RedisDB { get; set; }
}

public class dbAccountInfo
{
    public Int64 uid { get; set; }
    public string email { get; set; }
    public string pw { get; set; }
    public string salt_value { get; set; }
    public string create_date { get; set; }
    public string recent_login_date { get; set;}
}
