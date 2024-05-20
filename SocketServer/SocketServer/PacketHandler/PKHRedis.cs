using CloudStructures;
using CloudStructures.Structures;
using SocketLibrary;
using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PacketHandler;

public class PKHRedis
{
    public static Action<PacketData> DistributeFunc;
    public static Func<string, byte[], bool> SendFunc;

    protected ILog HandlerLogger;
    PacketToBytes PacketMaker = new PacketToBytes();

    public void Init(ILog logger)
    {
        HandlerLogger = logger;
    }

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData, RedisConnection>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.ReqDbLogin, RequestDBLogin);
    }

    //GameRedis에서 소켓서버 로그인할때 ID, authToken 비교하는거임. mysql은 노노.
    public void RequestDBLogin(PacketData packetData, RedisConnection redisConnection)
    {
        var user = MemoryPackSerializer.Deserialize<PKTReqDBLogin>(packetData.BodyData);

        try
        {
            var Result = CheckUserInfoFromRedis(user, redisConnection);
            HandlerLogger.Debug($"RedisDBLogin Result: {Result}");

            //ResponseDBLogin(packetData.SessionID, Result);

            if (Result == ErrorCode.None)
            {
                var data = new PKTReqLogin()
                {
                    UserID = user.Id,
                    AuthToken = user.AuthToken,
                    SessionID = packetData.SessionID
                };

                var body = MemoryPackSerializer.Serialize(data);
                var sendData = new PacketData();
                sendData.Assign((short)PACKETID.ReqLogin, body);
                DistributeFunc(sendData);
            }

        }
        catch (Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }
    }

    public ErrorCode CheckUserInfoFromRedis(PKTReqDBLogin user, RedisConnection redisConnection)
    {
        ErrorCode errorCode = ErrorCode.None;

        if (user == null)
        {
            return ErrorCode.DbLoginEmptyUser;
        }
        else
        {
            RedisString<RedisDBAuthUserData> redis = new(redisConnection, user.Id, null);
            var userAuthData = redis.GetAsync().Result;//await 사용 X 동기 처리 하기 위함

            HandlerLogger.Debug($"{userAuthData}");

            if (!userAuthData.HasValue)
            {
                return ErrorCode.DbLoginEmptyUser;
            }

            if (userAuthData.Value.AuthToken != user.AuthToken)
            {
                return ErrorCode.LoginInvalidAuthToken;
            }
        }

        return errorCode;
    }


    public void ResponseDBLogin(string sessionID, ErrorCode errorCode)
    {
        var data = new PKTResDBLogin()
        {
            Result = (short)errorCode
        };

        var bodyData = MemoryPackSerializer.Serialize(errorCode);
        var packetData = PacketMaker.MakePacket(PACKETID.ResLogin, bodyData);

        SendFunc(sessionID, packetData);
    }
}

public class RedisDBAuthUserData
{
    public string ID { get; set; } = "";
    public string AuthToken { get; set; } = "";

    public override string ToString()
    {
        return $"ID: {ID}, AuthToken: {AuthToken}";
    }
}
