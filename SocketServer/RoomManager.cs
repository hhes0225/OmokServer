using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class RoomManager
{
    List<Room> RoomList = new List<Room>();
    MainServer ServerNetwork;

    private Timer _checkRoomStateTimer;
    private Timer _checkTurnStateTimer;
    private int RoomTimeSpan, GameTimeSpan, GameTurnTimeSpan;

    public static Func<string, int> RemoveUserFromRoom;

    public RoomManager(MainServer mainServer)
    {
        ServerNetwork = mainServer;
        PKHRoom.CheckRoomStateFunc = this.CheckRoomState;
        PKHRoom.CheckGameStateFunc = this.CheckGameState;
        PKHOmokGame.CheckTurnStateFunc = this.CheckTurnState;

        SetTimeSpans(10, 1, 10);
    }


    public void SetTimeSpans(int roomTimeSpan, int gameTimeSpan, int gameTurnTimeSpan)
    {
        RoomTimeSpan = roomTimeSpan;
        GameTimeSpan = gameTimeSpan;
        GameTurnTimeSpan = gameTurnTimeSpan;
    }

    public void CreateRooms()
    {
        var maxRoomCount = ServerNetwork.ServerOption.RoomMaxCount;
        var startNumber = ServerNetwork.ServerOption.RoomStartNumber;
        var maxUserCount = ServerNetwork.ServerOption.RoomMaxUserCount;

        for(int i=0; i<maxRoomCount; i++)
        {
            var roomNubmer = startNumber + i;
            var room = new Room();
            room.Init(i, roomNubmer, maxUserCount);
            room.InitTimeSpan(RoomTimeSpan, GameTimeSpan, GameTurnTimeSpan);
            RoomList.Add(room);
        }

        InitAndStartRoomTimer(0, 5000);
        InitAndStartTurnTimer(0, 250);
        SetTimeSpans(1, 1, 0);
    }


    public void InitAndStartRoomTimer(int startTime, int interval)
    {
        TimerCallback callback= new TimerCallback(SendRoomCheckPkt);
        _checkRoomStateTimer = new Timer(callback, null, startTime, interval);
    }

    public void InitAndStartTurnTimer(int startTime, int interval)
    {
        TimerCallback callback = new TimerCallback(SendTurnCheckPkt);
        _checkTurnStateTimer= new Timer(callback, null, startTime, interval);
    }

    public void SendRoomCheckPkt(object state)
    {
        var ntfPkt = new PKTNtfInnerRoomCheck();
        var body = MemoryPackSerializer.Serialize(ntfPkt);

        var internalPacket = new PacketData();
        internalPacket.Assign((int)PACKETID.NTF_INNER_ROOM_CHECK, body);

        ServerNetwork.Distribute(internalPacket);
    }

    public void SendTurnCheckPkt(object state)
    {
        var ntfPkt = new PKTNtfInnerTurnCheck();
        var body = MemoryPackSerializer.Serialize(ntfPkt);

        var internalPacket = new PacketData();
        internalPacket.Assign((int)PACKETID.NTF_INNER_TURN_CHECK, body);
        
        ServerNetwork.Distribute(internalPacket);
    }

    public void CheckRoomState(int beginIndex, int endIndex)
    {
        if (endIndex > RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for(int i = beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].OmokBoard.GameFinish==false)
            {
                continue;
            }
            
            if (RoomList[i].IsRoomUsing == false)
            {
                continue;
            }

            //Room의 체크방 크리에이트
            if (RoomList[i].IsRoomCreatedButNotPlaying(curTime) == true)
            {
                continue;
            }

            //해당 룸의 모든 사람들 쫓아내
            RoomList[i].RemoveAllUser();
        }
    }
    

    public void CheckGameState(int beginIndex, int endIndex)
    {
        if (endIndex > RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for (int i = beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].OmokBoard.GameFinish == true)
            {
                continue;
            }

            if (RoomList[i].IsRoomUsing == false)
            {
                continue;
            }

            //Room의 체크게임스타트
            if (RoomList[i].IsGamePlayingTooLong(curTime) == true)
            {
                continue;
            }

            RoomList[i].NotifyEndOmok("");//게임 draw로 끝내
            RoomList[i].RemoveAllUser();//쫓아내
        }
    }

    public void CheckTurnState(int beginIndex, int endIndex)
    {
        if (endIndex > RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for(int i=beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].OmokBoard.GameFinish == true)
            {
                continue;
            }

            if (RoomList[i].IsRoomUsing == false)
            {
                continue;
            }

            if (RoomList[i].OmokBoard.IsPutStoneTooLong(curTime) == true)
            {
                continue;
            }

            //턴 강제 넘기기
            RoomList[i].NotifyPacketTurnPass();

            Console.WriteLine($"blackPCount: {RoomList[i].OmokBoard.BlackPassCount}");
            Console.WriteLine($"whitePCount: {RoomList[i].OmokBoard.WhitePassCount}");

            //만약 blackTurn>2 &&  whiteTurn>2라면?(둘 다 ->둘다 쫓아내고 크기에 따라 승패결정
            if (RoomList[i].OmokBoard.BlackPassCount+ RoomList[i].OmokBoard.WhitePassCount>=4
               && RoomList[i].OmokBoard.BlackPassCount>1 && RoomList[i].OmokBoard.WhitePassCount > 1)
            {
                RoomList[i].NotifyEndOmok("");//게임 draw로 끝내
                RoomList[i].RemoveAllUser();//쫓아내
            }
            else if(RoomList[i].OmokBoard.BlackPassCount + RoomList[i].OmokBoard.WhitePassCount >=4
                && RoomList[i].OmokBoard.BlackPassCount != RoomList[i].OmokBoard.WhitePassCount)
            {
                //else if 만약 blackTurn>2 || whiteTurn>2라면?(둘중 트롤만 쫓아내고 안트롤 승
                var goodUserID = "";
                var badUserID = "";

                if(RoomList[i].OmokBoard.BlackPassCount > RoomList[i].OmokBoard.WhitePassCount)
                {
                    goodUserID = RoomList[i].OmokBoard.WhitePlayerID;
                    badUserID = RoomList[i].OmokBoard.BlackPlayerID;
                }
                else
                {
                    goodUserID = RoomList[i].OmokBoard.BlackPlayerID;
                    badUserID = RoomList[i].OmokBoard.WhitePlayerID;
                }

                Console.WriteLine($"GoodUser: {goodUserID}");
                Console.WriteLine($"BadUser: {badUserID}");

                RoomList[i].NotifyEndOmok(goodUserID);
                RoomList[i].RemoveUser(RoomList[i].GetUserByNetSessionID(badUserID));
            }
            else
            {
                continue;
            }
        }
    }

    //마찬가지로 Room의 체크게임스타트 함수

    public List<Room> GetRoomList()
    {
        return RoomList;
    }

}
