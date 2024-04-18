using System;
using System.Data;
using SqlKata.Execution;
//QueryFactory 사용하기 위함(그냥 SqlKata는 안됨)
using HiveServer.Services;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Mysqlx.Connection;


namespace HiveServer.Repository;

public class AccountDB:IAccountDB
{
	//SQL DB connection 시 필요 변수
	private readonly IOptions<DatabaseConnection> _dbConfig;
	private IDbConnection _dbConnection;
	private readonly SqlKata.Compilers.MySqlCompiler _compiler;
	private readonly QueryFactory _queryFactory;
	private readonly ILogger<AccountDB> _logger;

    //생성자
    //MySQL - HiveDB와 connect
    //IOptions: ASP.NET Core에서 옵션을 구성하고 주입하기 위한 기능
    public AccountDB(ILogger<AccountDB> logger, IOptions<DatabaseConnection> dbConfig)
	{
		_dbConfig = dbConfig;
		_logger = logger;

		Open();//DB connect private 메소드

		_compiler = new SqlKata.Compilers.MySqlCompiler();
		_queryFactory = new QueryFactory(_dbConnection, _compiler);

	}

	//Hive database의 Account 테이블과 연결하기 위한 클래스
	public class DatabaseConnection
	{
		public string AccountDB { get; set; }
	}
	

	//HiveDB 서버와 Disconnect
	public void Dispose()
	{
		Close();
	}

	//인터페이스에서 상속받은 클래스 구현 - 자세한 설명은 IAccountDB에
    public Task<ErrorCode> CreateAccountAsync(string id, string pw)
	{

	}
    public Task<Tuple<ErrorCode, Int64>> VerifyUser(String email, string pw)
	{

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


	//dbconn과 dbconfig 차이
}
