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
	//SQL DB connection 시 필요 변수
	private readonly IOptions<DBConfig> _dbConfig;
	private IDbConnection _dbConnection;
	private readonly SqlKata.Compilers.MySqlCompiler _compiler;
	private readonly QueryFactory _queryFactory;
	private readonly ILogger<AccountDB> _logger;

    //생성자
    //MySQL - HiveDB와 connect
    //IOptions: ASP.NET Core에서 옵션을 구성하고 주입하기 위한 기능
    public AccountDB(ILogger<AccountDB> logger, IOptions<DBConfig> dbConfig)
	{
		_dbConfig = dbConfig;
		_logger = logger;

		Open();//DB connect private 메소드

		_compiler = new SqlKata.Compilers.MySqlCompiler();
		_queryFactory = new QueryFactory(_dbConnection, _compiler);

	}

	//Hive database의 Account 테이블과 연결하기 위한 클래스

	

	//HiveDB 서버와 Disconnect
	public void Dispose()
	{
		Close();
	}

	//인터페이스에서 상속받은 클래스 구현 - 자세한 설명은 IAccountDB에
    public async Task<ErrorCode> CreateAccountAsync(string id, string pw)
	{
		////일단 DB에 있는지 조회
		//try 
		//{
		//	int count = await _queryFactory.Query("Account").Where("email", id).Count<dynamic>();

		//	if (count == 0)
		//	{
		//		return ErrorCode.None;
		//	}
		//}
		//catch(Exception e) 
		//{
		//	return ErrorCode.CreateAccountFailException;
		//}

		return ErrorCode.None;//임의로 넣은 것
	}
    public async Task<Tuple<ErrorCode, Int64>> VerifyUser(String email, string pw)
	{
        return new Tuple<ErrorCode, long>(ErrorCode.None, (long)0);//임의로 넣은 것
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
}
