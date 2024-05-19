using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using ZLogger;

namespace GameServer.Repository;

public class MemoryDB:IMemoryDB
{
    private readonly ILogger<MemoryDB> _logger;
    public RedisConnection _redisConn;

    public MemoryDB(ILogger<MemoryDB>logger, IOptions<DBConfig> dbConfig)
    {
        _logger = logger;
        RedisConfig redisConfig = new RedisConfig("default", dbConfig.Value.RedisDB);
        _redisConn=new RedisConnection(redisConfig);
    }
    public void Dispose()
    {

    }

    public async Task<ErrorCode> RegisterUserAsync(string email, string authToken)
    {
        ErrorCode errorCode = ErrorCode.None;

        RedisDBAuthUserData user = new()
        {
            Id = email,
            AuthToken = authToken
        };

        string keyValue = user.Id;

        try
        {
            RedisString<RedisDBAuthUserData> redis = new(_redisConn, keyValue, LoginTimeSpan());
            if(await redis.SetAsync(user, LoginTimeSpan()) == false)
            {
                return ErrorCode.LoginFailAddRedis;
            }
        }
        catch
        {
            return ErrorCode.RedisFailException;
        }

        return errorCode;
    }

    public async Task<ErrorCode> CheckUserAuthAsync(string email, string authToken)
    {
        ErrorCode errorCode = ErrorCode.None;
        return errorCode;
    }

    public TimeSpan LoginTimeSpan()
    {
        return TimeSpan.FromHours(RedisKeyExpireTime.LoginKeyExpireHour);
    }
}

public class RedisKeyExpireTime
{
    public const ushort LoginKeyExpireHour = 6;
}

public class RedisDBAuthUserData
{
    public string Id { get; set; } = "";
    public string AuthToken { get; set; } = "";
}
