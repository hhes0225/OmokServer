using System;
using System.Data;
using SqlKata.Execution;
using APIServer.Services;
using Microsoft.Extensions.Options;
using MySqlConnector;
using ZLogger;


namespace APIServer.Repository;

public class AccountDB:IAccountDB
{
	private readonly IOptions<DBConfig> _dbConfig;
	private IDbConnection _dbConnection;
	private readonly SqlKata.Compilers.MySqlCompiler _compiler;
	private readonly QueryFactory _queryFactory;
	private readonly ILogger<AccountDB> _logger;

    //MySQL - HiveDB와 connect
    public AccountDB(ILogger<AccountDB> logger, IOptions<DBConfig> dbConfig)
	{
		_dbConfig = dbConfig;
		_logger = logger;

		Open();

		_compiler = new SqlKata.Compilers.MySqlCompiler();
		_queryFactory = new QueryFactory(_dbConnection, _compiler);

	}

	public void Dispose()
	{
		Close();
	}

    //Hive DB에 해당 계정 존재 여부 조회(계정 생성 요청 시 호출)
    public async Task<ErrorCode> FindAccountExistAsync(string email)
	{  
        try
        {
            long count = _queryFactory.Query("Account").Where("email", email).Count<dynamic>();

            if (count != 0)
            {
                return ErrorCode.AccountAlreadyExist;
                Console.WriteLine("account is already exist");
            }
        }
        catch (Exception e)
        {
            return ErrorCode.FindAccountExistException;
        }

        return ErrorCode.None;
    }

    //DB에 사용자 추가(계정 생성 시 호출)
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

            if (count != 1) {
                return ErrorCode.CreateAccountDuplicatedUser;
            }

        }
        catch(Exception e)
        {
            return ErrorCode.CreateAccountFailException;
        }

		return ErrorCode.None;
	}

	//사용자 인증(로그인 시 호출)
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

            //비밀번호 비교
            //들어온 비밀번호에 같은 로직으로 암호화했을 때, db 비밀번호와 같은지 확인
            string hashedPasword = Security.HashPassword(accountInfo.salt_value, pw);
            if(hashedPasword != accountInfo.pw)
            {
                return  new Tuple <ErrorCode, string> (ErrorCode.LoginFailWrongPassword, "0");
            }

            //최근 로그인 시간 현재 시간으로 변경
            string newDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            int count = _queryFactory.Query("Account").Where("email", email).Update(new {recent_login_date=newDate});

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
		//Console.WriteLine(_dbConfig.Value.AccountDB);
        _dbConnection.Open();
    }


    private void Close()
	{
		_dbConnection.Close();
	}


	//dbconn과 dbconfig 차이
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
