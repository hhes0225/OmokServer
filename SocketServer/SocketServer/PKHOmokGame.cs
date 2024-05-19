using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class PKHOmokGame:PKHandler
{
    List<Room> RoomList = null;
    int StartRoomNumber;
    PacketToBytes PacketMaker = new PacketToBytes();

    private int _startIndexRoomCheck = 0;
    private const int MaxCheckRoomCount = 50;


    public void SetRoomList(List<Room> roomList)
    {
        RoomList = roomList;
        StartRoomNumber = roomList[0].Number;
    }

    Room GetRoom(int roomNumber)
    {
        var index = roomNumber - StartRoomNumber;
        if (index < 0 || index >= RoomList.Count())
        {
            return null;
        }

        return RoomList[index];
    }

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PACKETID.ReqReadyOmok, RequestUserReady);
        packetHandlerMap.Add((int)PACKETID.ReqPutOmok, RequestPutOmok);
    }



    public void RequestUserReady(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        HandlerLogger.Debug("Ready request");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqReadyOmok>(packetData.BodyData);
            var room = GetRoom(reqData.RoomNumber);

            if (room == null)
            {
                HandlerLogger.Debug("유효하지 않은 방");
                return;
            }

            var roomUser = room.GetUserByNetSessionID(sessionID);
            if (roomUser == null)
            {
                HandlerLogger.Debug("유저 존재하지 않음");
                return;
            }

            if (roomUser.GetUserState() == UserState.Ready)
            {
                HandlerLogger.Debug("유저 이미 준비상태임");
            }
            else 
            { 
                roomUser.Ready();
            }

            room.NotifyPacketUserReady(roomUser.UserID);
            ResponseUserReady(sessionID);

            if(IsGameStartPossible(room) == true)
            {
                NotifyGameStart(room);
            }

            HandlerLogger.Debug($"{roomUser.UserID} is Ready");

        }
        catch(Exception ex)
        {
            HandlerLogger.Error(ex.ToString());
        }
    }

    public void ResponseUserReady(string sessionID)
    {
        var resUserReady = new PKTResReadyOmok()
        {
            Result = (Int16)ERROR_CODE.None
        };

        var body = MemoryPackSerializer.Serialize(resUserReady);
        var sendData = PacketMaker.MakePacket(PACKETID.ResReadyOmok, body);

        SendDataFunc(sessionID, sendData);
    }

    public bool IsGameStartPossible(Room room)
    {
        var roomUserList = room.GetUserList();

        if(roomUserList.Count < room.MaxUserCount)
        {
            return false;
        }
        
        foreach (var roomUser in roomUserList)
        {
            if(roomUser.GetUserState() != UserState.Ready)
            {
                return false;
            }
        }

        return true;
    }

    public void NotifyGameStart(Room room)
    {
        var random = new Random();
        var userList = room.GetUserList();

        var randomBlackIndex = random.Next(0, room.CurrentUserCount());
        
        var packet = new PKTNtfStartOmok();
        packet.BlackUserID = userList[randomBlackIndex].UserID;
        packet.WhiteUserID = userList[room.CurrentUserCount() - randomBlackIndex-1].UserID;

        var bodyData = MemoryPackSerializer.Serialize(packet);

        var sendData = PacketMaker.MakePacket(PACKETID.NtfStartOmok, bodyData);
        HandlerLogger.Debug("Game Start...");

        room.Broadcast("", sendData);

        room.StartGame();
        room.OmokBoard.SetPlayerColor(userList[randomBlackIndex].NetSessionID, userList[room.CurrentUserCount() - randomBlackIndex - 1].NetSessionID); ;
        HandlerLogger.Debug($"Game Start Time: {room.GameStartTime}");
    }

    public void RequestPutOmok(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        HandlerLogger.Debug("돌 두기 요청");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqPutMok>(packetData.BodyData);
            var turnPlayer = sessionID;
            var room = GetRoom(reqData.RoomNumber);

            //게임 아직 시작하지 않았다면 돌려보내기(NOT STARTED)
            if (room.OmokBoard.GameFinish == true)
            {
                ResponsePutOmok(ERROR_CODE.OmokNotStarted, turnPlayer);
                return;
            }

            //만약 올바른 턴 유저가 보낸 것이 아니라면 돌려보내기
            if (room.OmokBoard.WhoseTurn() != turnPlayer)
            {
                ResponsePutOmok(ERROR_CODE.OmokTurnNotMatch, turnPlayer);
                return;
            }
            
            //putomok으로 이 자리에 돌을 둘 수 있는지 확인
            //true면 서버에서 돌 두기, 클라에 돌 둘 수 있다고 반환(None)
            //false면 서버에서 돌 두지 않고 클라에 리스폰스(ALREADY EXIST)

            if(room.OmokBoard.CheckAvailablePosition(reqData.PosX, reqData.PosY)==false)
            {
                ResponsePutOmok(ERROR_CODE.OmokAlreadyExist, turnPlayer);
            }

            var putStone = room.OmokBoard.PutStone(reqData.PosX, reqData.PosY);

            //response message 보내서 돌 두기
            ResponsePutOmok(ERROR_CODE.None, turnPlayer);

            var notifyPutOmok = new PKTNtfPutMok()
            {
                PosX = reqData.PosX,
                PosY = reqData.PosY,
                Mok = room.OmokBoard.CurTurnCount
            };

            var body = MemoryPackSerializer.Serialize(notifyPutOmok);
            var sendData = PacketMaker.MakePacket(PACKETID.NtfPutOmok, body);

            room.Broadcast("", sendData);

            //승리조건 판별
            //승리했다면 Notify하고 EndGame
            //만약 CurTurnCount가 19*19라면 draw
            if (room.OmokBoard.CheckWinningCondition(reqData.PosX,reqData.PosY)==true)
            {
                room.NotifyEndOmok(sessionID);
            }
            else if (room.OmokBoard.CurTurnCount >= 19 * 19)
            {
                //draw
                room.NotifyEndOmok("");
            }
            else
            {
                HandlerLogger.Debug("게임 계속 진행...");
            }
        }
        catch (Exception ex) 
        {
            HandlerLogger.Error(ex.ToString());
        }
    }

    

    public void ResponsePutOmok(ERROR_CODE errorCode, string sessionID)
    {
        var resPutOmok = new PKTResPutMok()
        {
            Result = (Int16)errorCode
        };

        var body = MemoryPackSerializer.Serialize(resPutOmok);
        var sendData = PacketMaker.MakePacket(PACKETID.ResPutOmok, body);

        SendDataFunc(sessionID, sendData);
    }

    //public void NotifyEndOmok(Room room, string sessionID)
    //{
    //    room.SetAllInitState();
    //    room.EndGame();

    //    var ntfEndOmok = new PKTNtfEndOmok();

    //    if (sessionID != "")
    //    {
    //        var winner = room.GetUserByNetSessionID(sessionID);
    //        ntfEndOmok.WinUserID = winner.UserID;
    //    }
    //    else
    //    {
    //        ntfEndOmok.WinUserID = room.OmokBoard.DrawGame();
    //    }
        
    //    var body = MemoryPackSerializer.Serialize(ntfEndOmok);
    //    var sendData = PacketMaker.MakePacket(PACKETID.NTF_END_OMOK, body);

    //    room.Broadcast("", sendData);

    //}
}
