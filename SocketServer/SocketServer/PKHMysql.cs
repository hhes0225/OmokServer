﻿using CSBaseLib;
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
            result = queryFactory.Query("User").Where("id", gameResult.Winner).Increment("draw_count", 1);

            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdateFail;
            }

            result = queryFactory.Query("User").Where("id", gameResult.Winner).Increment("draw_count", 1);
            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdateFail;
            }
        }
        else
        {
            result = queryFactory.Query("User").Where("id", gameResult.Winner).Increment("win_count", 1);
            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdateFail;
            }

            result = queryFactory.Query("User").Where("id", gameResult.Loser).Increment("lose_count", 1);
            if (result != 1)
            {
                return ERROR_CODE.DbGameResultUpdateFail;
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
                id = testUser.Id,
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