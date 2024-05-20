using SocketLibrary;
using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketServer.PacketHandler;
using SocketServer.Game;

namespace SocketServer.RoomDir;

public class RoomManager
{
    List<Room> RoomList = new List<Room>();
    //MainServer ServerNetwork;
    ServerOption ServerOption;

    private Timer _checkRoomStateTimer;
    private Timer _checkTurnStateTimer;
    private int RoomTimeSpan, GameTimeSpan, GameTurnTimeSpan;

    //public static Func<string, int> RemoveUserFromRoom;
    public static Action<PacketData> SendInternalFunc;

    public ILog RoomMgrLogger;

    public RoomManager(MainServer mainServer)
    {
        ServerOption = mainServer.ServerOption;
        PKHRoom.CheckRoomStateFunc = CheckRoomState;
        PKHRoom.CheckGameStateFunc = CheckGameState;
        PKHRoom.CheckTurnStateFunc = CheckTurnState;
        RoomMgrLogger = mainServer.MainLogger;

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
        var maxRoomCount = ServerOption.RoomMaxCount;
        var startNumber = ServerOption.RoomStartNumber;
        var maxUserCount = ServerOption.RoomMaxUserCount;

        for (int i = 0; i < maxRoomCount; i++)
        {
            var roomNubmer = startNumber + i;
            var room = new Room();
            room.Init(i, roomNubmer, maxUserCount);
            room.InitTimeSpan(RoomTimeSpan, GameTimeSpan);
            RoomList.Add(room);
        }

        InitAndStartRoomTimer(0, 250);
        SetTimeSpans(1, 1, 0);
    }


    public void InitAndStartRoomTimer(int startTime, int interval)
    {
        TimerCallback callback = new TimerCallback(SendRoomAndTurnCheckPkt);
        _checkRoomStateTimer = new Timer(callback, null, startTime, interval);
    }


    public void SendRoomAndTurnCheckPkt(object state)
    {
        var ntfPkt = new PKTNtfInRoomCheck();
        var body = MemoryPackSerializer.Serialize(ntfPkt);

        var internalPacket = new PacketData();
        internalPacket.Assign((int)PACKETID.NtfInRoomCheck, body);

        SendInternalFunc(internalPacket);
    }

    public void CheckRoomState(int beginIndex, int endIndex)
    {
        if (endIndex >= RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for (int i = beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].IsRoomUsing == true && RoomList[i].IsRoomCreatedButNotPlaying(curTime) == true)
            {
                //해당 룸의 모든 사람들 쫓아냄
                RoomList[i].RemoveAllUser();
            }
        }
    }


    public void CheckGameState(int beginIndex, int endIndex)
    {
        if (endIndex >= RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for (int i = beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].IsRoomUsing == true && RoomList[i].IsGamePlayingTooLong(curTime) == true)
            {
                RoomList[i].NotifyEndOmok("");//게임 draw로 끝내
                RoomList[i].RemoveAllUser();//쫓아내
            }
        }
    }

    public void CheckTurnState(int beginIndex, int endIndex)
    {
        if (endIndex >= RoomList.Count)
        {
            endIndex = RoomList.Count;
        }

        var curTime = DateTime.Now;

        for (int i = beginIndex; i < endIndex; i++)
        {
            if (RoomList[i].IsRoomUsing == true && RoomList[i].OmokBoard.IsPutStoneTooLong(curTime) == true)
            {
                //턴 강제 넘기기
                RoomList[i].NotifyClientTurnPass();

                RoomMgrLogger.Debug($"blackPCount: {RoomList[i].OmokBoard.BlackPassCount}");
                RoomMgrLogger.Debug($"whitePCount: {RoomList[i].OmokBoard.WhitePassCount}");

                var totalPass = RoomList[i].OmokBoard.BlackPassCount + RoomList[i].OmokBoard.WhitePassCount;
                if (totalPass >= 4)
                {
                    ForcedEndGame(RoomList[i]);
                }
            }

        }
    }


    public void ForcedEndGame(Room room)
    {
        //만약 blackTurn>2 &&  whiteTurn>2라면?(둘 다 ->둘다 쫓아내고 크기에 따라 승패결정
        if (room.OmokBoard.BlackPassCount == room.OmokBoard.WhitePassCount)
        {
            room.NotifyEndOmok("");//게임 draw로 끝내
            room.RemoveAllUser();//쫓아내
        }
        else if (room.OmokBoard.BlackPassCount != room.OmokBoard.WhitePassCount)
        {
            //else if 만약 blackTurn>2 || whiteTurn>2라면?(둘중 트롤만 쫓아내고 안트롤 승
            var goodUserID = "";
            var badUserID = "";

            if (room.OmokBoard.BlackPassCount > room.OmokBoard.WhitePassCount)
            {
                goodUserID = room.OmokBoard.WhitePlayerID;
                badUserID = room.OmokBoard.BlackPlayerID;
            }
            else
            {
                goodUserID = room.OmokBoard.BlackPlayerID;
                badUserID = room.OmokBoard.WhitePlayerID;
            }

            RoomMgrLogger.Debug($"GoodUser: {goodUserID}");
            RoomMgrLogger.Debug($"BadUser: {badUserID}");

            room.NotifyEndOmok(goodUserID);
            room.RemoveUser(room.GetUserByNetSessionID(badUserID));
        }
        else
        {
            return;
        }
    }

    public List<Room> GetRoomList()
    {
        return RoomList;
    }

}
