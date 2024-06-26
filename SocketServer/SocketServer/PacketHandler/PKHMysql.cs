﻿using SocketLibrary;
using MemoryPack;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PacketHandler;

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
        packetHandlerMap.Add((int)PACKETID.ReqInInsertTestUser, RequestInternalInsertTestData);
    }

    public void NotifyInternalGameResultUpdate(PacketData packetData, QueryFactory queryFactory)
    {
        try
        {
            var result = GameResultUpdate(packetData, queryFactory);
            HandlerLogger.Debug($"Game result database update result : {result}");
        }
        catch (Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }

    }

    public ErrorCode GameResultUpdate(PacketData packetData, QueryFactory queryFactory)
    {
        var gameResult = MemoryPackSerializer.Deserialize<PKTNtfInGameResultUpdate>(packetData.BodyData);
        var result = 0;

        if (gameResult.IsDraw == true)
        {
            result = queryFactory.Query("User").Where("id", gameResult.Winner).Increment("draw_count", 1);

            if (result != 1)
            {
                return ErrorCode.DbGameResultUpdateFail;
            }

            result = queryFactory.Query("User").Where("id", gameResult.Loser).Increment("draw_count", 1);
            if (result != 1)
            {
                return ErrorCode.DbGameResultUpdateFail;
            }
        }
        else
        {
            result = queryFactory.Query("User").Where("id", gameResult.Winner).Increment("win_count", 1);
            if (result != 1)
            {
                return ErrorCode.DbGameResultUpdateFail;
            }

            result = queryFactory.Query("User").Where("id", gameResult.Loser).Increment("lose_count", 1);
            if (result != 1)
            {
                return ErrorCode.DbGameResultUpdateFail;
            }
        }
        HandlerLogger.Debug($"{packetData.SessionID} : Game result DB 업데이트 완료");

        return ErrorCode.None;
    }

    public void RequestInternalInsertTestData(PacketData packetData, QueryFactory queryFactory)
    {
        try
        {
            var testUser = MemoryPackSerializer.Deserialize<PKTReqInInsertTestUser>(packetData.BodyData);

            var result = queryFactory.Query("User").Insert(new
            {
                id = testUser.Id,
                nickname = ".",
                win_count = testUser.WinCount,
                draw_count = testUser.DrawCount,
                lose_count = testUser.LoseCount
            });

            ResponseInternalInsertTestData(result);

        }
        catch (Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }
    }

    public void ResponseInternalInsertTestData(int result)
    {
        var data = new PKTResInInsertTestUser();

        if (result != 1)
        {
            data.Result = (short)ErrorCode.DbAlreadyExistUser;
        }
        else
        {
            data.Result = (short)ErrorCode.None;
        }

        var bodyData = MemoryPackSerializer.Serialize(data);
        var packetData = new PacketData();
        packetData.Assign((short)PACKETID.ResInInsertTestUser, bodyData);
        DistributeFunc(packetData);
    }

}
