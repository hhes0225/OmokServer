using System;
using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using ZLogger;


namespace GameServer.Repository;

public class UserDB : IUserDB
{
    private readonly ILogger<UserDB> _logger;
    private readonly IOptions<DBConfig> _dbConfig;
    private IDbConnection _dbConnection;
    private readonly SqlKata.Compilers.MySqlCompiler _compiler;
    private readonly QueryFactory _queryFactory;

    public UserDB(ILogger<UserDB> logger, IOptions<DBConfig> dbConfig)
    {
        _logger = logger;
        _dbConfig = dbConfig;

        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConnection, _compiler);
    }

    public void Dispose()
    {
        Close();
    }

    public async Task<dbUserInfo> FindUserDataAsync(string email)
    {
        return await _queryFactory.Query("User").
            Where("email", email).
            FirstOrDefaultAsync<dbUserInfo>();
    }

    public async Task<ErrorCode> InsertUserAsync(string email)
    {
        int count = await _queryFactory.Query("User").InsertAsync(new
        {
            email = email,
            nickname = "",
            create_date = DateTime.Now,
            recent_login_date = DateTime.Now
        });

        if(count != 1)
        {
            return ErrorCode.UserDBInsertException;
        }

        return ErrorCode.None;
    }

    public async Task<int> UpdateRecentLogin(int uid)
    {
        return await _queryFactory.Query("user").Where("uid", uid).UpdateAsync(new
        {
            recent_login_dt = DateTime.Now,
        });
    }

    private void Open()
    {
        _dbConnection = new MySqlConnection(_dbConfig.Value.UserDB);
        _dbConnection.Open();

    }

    private void Close()
    {
        _dbConnection.Close();
    }
}

public class DBConfig
{
    public string UserDB { get; set; }
    public string RedisDB { get; set; }
}

public class dbUserInfo
{
    public Int64 uid { get; set; }
    public string email {  get; set; }
    public string nickname {  get; set; }
    public string create_date {  get; set; }
    public string recent_login_date {  get; set; }
    public int win_count { get; set; } = 0;
    public int draw_count { get; set; } = 0;
    public int lose_count { get; set; } = 0;

    public override string ToString()
    {
        return $"uid: {uid}, Email: {email}"; // 필요한 속성들을 포함하여 원하는 형식으로 출력
    }
}


