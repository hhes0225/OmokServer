using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CSBaseLib;
using MemoryPack;
using MySqlConnector;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;

namespace SocketServer;

public class MySqlProcessor
{
    bool IsThreadRunning = false;
    List<Thread> ProcessThread = new List<Thread>();
    string DbConfig = "Server=localhost; user=root; Port=3306; password = awsd95731.;Database=GameDB;";

    BufferBlock<PacketData> MsgBuffer;

    private SqlKata.Compilers.MySqlCompiler _compiler;

    //패킷 핸들러 등록
    Dictionary<int, Action<PacketData, QueryFactory>> PacketHandlerMap = new Dictionary<int, Action<PacketData, QueryFactory>>();
    PKHMysql MysqlPacketHandler = new PKHMysql();
    
    SuperSocket.SocketBase.Logging.ILog ProcessLogger;

    public MySqlProcessor(SuperSocket.SocketBase.Logging.ILog logger)
    {
        ProcessLogger = logger;
        MsgBuffer = new BufferBlock<PacketData>();
    }

    public void CreateAndStart(int threadNum)
    {
        //프로세서 패킷 핸들러 등록
        RegisterPacketHandler(ProcessLogger);

        //DB 컴파일러 두기
        _compiler = new SqlKata.Compilers.MySqlCompiler();

        //스레드 시작 관련 세팅
        IsThreadRunning = true;
        for (int i = 0; i < threadNum; i++)
        {
            ProcessThread.Add(new Thread(this.Process));
        }

        for (int i = 0; i < threadNum; i++)
        {
            ProcessThread[i].Start();
        }


        //테스트 유저 삽입
        //var data = new PKTNtfInInsertTestUser()
        //{
        //    Id = "qwer",
        //    WinCount = 2,
        //    DrawCount = 5,
        //    LoseCount = 1
        //};

        //var body = MemoryPackSerializer.Serialize(data);
        //var InternalPacket = new PacketData();
        //InternalPacket.Assign((short)PACKETID.NtfInInsertTestUser, body);
        //InsertPacket(InternalPacket);
    }

    public void Destroy()
    {
        IsThreadRunning=false;
        MsgBuffer.Complete();
    }

    public void InsertPacket(PacketData data)
    {
        MsgBuffer.Post(data);
    }

    void RegisterPacketHandler(ILog logger)
    {
        MysqlPacketHandler.Init(logger);
        MysqlPacketHandler.RegisterPacketHandler(PacketHandlerMap);
    }

    void Process()
    {
        IDbConnection dbConnection = new MySqlConnection(DbConfig);
        dbConnection.Open();
        //SQL 쿼리팩토리 생성
        QueryFactory queryFactory = new QueryFactory(dbConnection, _compiler);

        while (IsThreadRunning)
        {
            try
            {
                var packet = MsgBuffer.Receive();

                if (PacketHandlerMap.ContainsKey(packet.PacketID))
                {
                    PacketHandlerMap[packet.PacketID](packet, queryFactory);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"MySqlProcessor - 세션 번호: {packet.SessionID}, PacketID {packet.PacketID}," +
                        $"받은 데이터 크기: {packet.BodyData.Length}");
                }
            }
            catch(Exception ex) 
            {
                IsThreadRunning.IfTrue(() => ProcessLogger.Error(ex.ToString()));
            }
        }
    }
}
