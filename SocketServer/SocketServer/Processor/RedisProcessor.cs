using CloudStructures;
using SocketServer.PacketHandler;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SocketServer.Processor;

public class RedisProcessor
{
    bool IsThreadRunning = false;
    List<Thread> ProcessThread = new List<Thread>();

    string rConfig;

    BufferBlock<PacketData> MsgBuffer;

    //패킷 핸들러 등록
    Dictionary<int, Action<PacketData, RedisConnection>> PacketHandlerMap = new Dictionary<int, Action<PacketData, RedisConnection>>();
    PKHRedis RedisPacketHandler = new PKHRedis();
    int uniqueID = 0;

    ILog ProcessLogger;

    public RedisProcessor(ILog logger, ServerOption serverOption)
    {
        ProcessLogger = logger;
        rConfig = serverOption.RedisConfig;
        MsgBuffer = new BufferBlock<PacketData>();
    }

    public void CreateAndStart(int threadNum)
    {
        //프로세서 패킷 핸들러 등록
        RegisterPacketHandler(ProcessLogger);

        //스레드 시작 관련 세팅
        IsThreadRunning = true;
        for (int i = 0; i < threadNum; i++)
        {
            ProcessThread.Add(new Thread(Process));
        }

        for (int i = 0; i < threadNum; i++)
        {
            ProcessThread[i].Start();
        }

    }

    public void Destroy()
    {
        IsThreadRunning = false;
        MsgBuffer.Complete();
    }

    public void InsertPacket(PacketData data)
    {
        MsgBuffer.Post(data);
    }

    void RegisterPacketHandler(ILog logger)
    {
        //패킷핸들러
        RedisPacketHandler.Init(logger);
        RedisPacketHandler.RegisterPacketHandler(PacketHandlerMap);
    }

    void Process()
    {
        //Redis 연결
        RedisConfig _redisConfig = new RedisConfig($"default{++uniqueID}", rConfig);
        RedisConnection _redisConnection = new RedisConnection(_redisConfig);

        while (IsThreadRunning)
        {
            try
            {
                var packet = MsgBuffer.Receive();

                if (PacketHandlerMap.ContainsKey(packet.PacketID))
                {
                    PacketHandlerMap[packet.PacketID](packet, _redisConnection);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"MySqlProcessor - 세션 번호: {packet.SessionID}, PacketID {packet.PacketID}," +
                        $"받은 데이터 크기: {packet.BodyData.Length}");
                }
            }
            catch (Exception ex)
            {
                IsThreadRunning.IfTrue(() => ProcessLogger.Error(ex.ToString()));
            }
        }
    }

}
