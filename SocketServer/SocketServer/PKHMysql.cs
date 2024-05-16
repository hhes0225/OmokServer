using CSBaseLib;
using MemoryPack;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class PKHMysql
{
    //public static Func<string, byte[], bool> SendDataFunc;
    public static Action<PacketData> DistributeFunc;

    protected SuperSocket.SocketBase.Logging.ILog HandlerLogger;

    public void Init(SuperSocket.SocketBase.Logging.ILog logger)
    {
        HandlerLogger = logger;
    }

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData, QueryFactory>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.NtfInGameResultUpdate, NotifyInternalGameResultUpdate);
        packetHandlerMap.Add((int)PACKETID.NtfInInsertTestUser, NotifyInternalInsertTestData);
    }
    
    ////GameRedis에서 소켓서버 로그인할때 ID, authToken 비교하는거임. mysql은 노노.
    //public void RequestDBLogin(PacketData packetData, QueryFactory queryFactory)
    //{

    //}

    //public void ResponseDBLogin(PacketData packetData, QueryFactory queryFactory)
    //{

    //}

    public void NotifyInternalGameResultUpdate(PacketData packetData, QueryFactory queryFactory)
    {

        try
        {
            var result = GameResultUpdate(packetData, queryFactory);
            HandlerLogger.Debug($"Game result database update result : {result}");
        }
        catch(Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }

    }

    //유저 정보 가져와서 이긴 사람, 진 사람, 비긴 사람 정보 업데이트
    public ERROR_CODE GameResultUpdate(PacketData packetData, QueryFactory queryFactory)
    {
        var gameResult = MemoryPackSerializer.Deserialize<PKTNtfInnerGameResultUpdate>(packetData.BodyData);
        var result=0;

        if (gameResult.IsDraw == true)
        {
            result = queryFactory.Query("User").Where("email", gameResult.Winner).Increment("draw_count", 1);

            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdatFail;
            }

            result = queryFactory.Query("User").Where("email", gameResult.Winner).Increment("draw_count", 1);
            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdatFail;
            }
        }
        else
        {
            result = queryFactory.Query("User").Where("email", gameResult.Winner).Increment("win_count", 1);
            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdatFail;
            }

            result = queryFactory.Query("User").Where("email", gameResult.Loser).Increment("lose_count", 1);
            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdatFail;
            }
        }
        HandlerLogger.Debug($"{packetData.SessionID} : Game result DB 업데이트 완료");

        return ERROR_CODE.None;
    }

    public void NotifyInternalInsertTestData(PacketData packetData, QueryFactory queryFactory) 
    {
        try
        {
            var testUser = MemoryPackSerializer.Deserialize<PKTNtfInInsertTestUser>(packetData.BodyData);

            queryFactory.Query("User").Insert(new
            {
                email = testUser.Id,
                nickname = ".",
                win_count=testUser.WinCount,
                draw_count=testUser.DrawCount,
                lose_count=testUser.LoseCount
            });

        }
        catch(Exception ex) 
        {
            HandlerLogger.Error(ex.ToString());
        }
        

    }

}
