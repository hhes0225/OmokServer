using HiveServer.Services;
using ZLogger;
using CloudStructures;
using CloudStructures.Structures;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace HiveServer.Repository;

public class MemoryDB : IMemoryDB
{
    private readonly ILogger<MemoryDB> _logger;
    public RedisConnection _redisConn;

    public MemoryDB(ILogger<MemoryDB> logger, IOptions<DBConfig> dbConfig)
    {
        _logger=logger;
        RedisConfig redisConfig = new RedisConfig("default", dbConfig.Value.RedisDB);
        _redisConn=new RedisConnection(redisConfig);

        _logger.ZLogDebug($"userDBAddress:{dbConfig.Value.RedisDB}");
    }

    public void Dispose()
    {
    }

    public async Task<ErrorCode> RegisterUserAsync(string email, string authToken)
    {
        return ErrorCode.None;
    }

    public async Task<ErrorCode> CheckUserAuthAsync(string email, string authToken)
    {
        return ErrorCode.None;
    }

    public async Task<Tuple<bool, RedisDBAuthUserData>> GetUserAsync(string email)
    {
       
        return new Tuple<bool, RedisDBAuthUserData> (true, null);
    }
}


public class RedisDBAuthUserData
{
    public string email { get; set; } = "";
    public string AuthToken { get; set; } = "";
}