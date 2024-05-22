using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace GameServer.Repository;

public class MatchingList:IMatchingList
{
    private readonly ILogger<MatchingList> _logger;
    public RedisConnection _redisConn;
    public const int MaxRoomNumber = 100;

    public MatchingList(ILogger<MatchingList> logger, IOptions<DBConfig> dbConfig)
    {
        _logger = logger;
        RedisConfig redisConfig = new RedisConfig("default", dbConfig.Value.RedisMatchingList);
        _redisConn = new RedisConnection(redisConfig);
    }

    public void Dispose()
    {

    }

    public async Task<MatchingResult> MatchingUserAsync()
    {
        MatchingResult matchingResult = new MatchingResult();

        //redis 키 incre한다.
        //키 짝수면 키/2 반환
        //키 홀수면 notyet 에러 반환
        //키/2가 방 최대면 다시 1번부터 시작

        try
        {
            RedisString<int> redisKey = new (_redisConn, "matchingKey", null);
            var incrementedKey = await redisKey.IncrementAsync();

            if (incrementedKey % 2 == 0)
            {
                matchingResult.RoomNumber = (incrementedKey / 2).ToString();

                // 키/2가 방 최대면 다시 1번부터 시작
                if ((incrementedKey / 2) > MaxRoomNumber)
                {
                    await redisKey.SetAsync(0);
                }

                matchingResult.Result = ErrorCode.None;
            }
            else
            {
                matchingResult.Result = ErrorCode.MatchingNotYet;
            }

        }
        catch
        {
            matchingResult.Result = ErrorCode.MatchingFailError;
        }

        return matchingResult;
    }
}
public class MatchingResult
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string RoomNumber { get; set; } = "";
}

