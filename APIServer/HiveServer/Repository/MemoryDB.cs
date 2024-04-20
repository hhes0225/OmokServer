using HiveServer.Services;
using ZLogger;
using CloudStructures;
using CloudStructures.Structures;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Bcpg;


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
        ErrorCode errorCode = ErrorCode.None;

        RedisDBAuthUserData user = new()
        {
            Email = email,
            AuthToken = authToken
        };

        string keyValue = user.Email;

        try
        {
            RedisString<RedisDBAuthUserData> redis = new(_redisConn, keyValue, LoginTimeSpan());

            if(await redis.SetAsync(user, LoginTimeSpan())==false)
            {
                return ErrorCode.LoginFailAddRedis;
            }
        }
        catch (Exception ex)
        {
            return ErrorCode.RedisFailException;
        }

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
    public string Email { get; set; } = "";
    public string AuthToken { get; set; } = "";
}