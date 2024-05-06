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

    //OmokRule OmokRule = new OmokRule();

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
        packetHandlerMap.Add((int)PACKETID.REQ_READY_OMOK, RequestUserReady);
        packetHandlerMap.Add((int)PACKETID.REQ_PUT_OMOK, RequestPutOmok);
    }

    public void RequestUserReady(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug("Ready request");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqReadyOmok>(packetData.BodyData);
            var room = GetRoom(reqData.RoomNumber);

            if (room == null)
            {
                ServerNetwork.MainLogger.Debug("유효하지 않은 방");
                return;
            }

            var roomUser = room.GetUserByNetSessionID(sessionID);
            if (roomUser == null)
            {
                ServerNetwork.MainLogger.Debug("유저 존재하지 않음");
                return;
            }

            if (roomUser.GetUserState() == UserState.Ready)
            {
                ServerNetwork.MainLogger.Debug("유저 이미 준비상태임");
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

            ServerNetwork.MainLogger.Debug($"{roomUser.UserID} is Ready");

        }
        catch(Exception ex)
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    public void ResponseUserReady(string sessionID)
    {
        var resUserReady = new PKTResReadyOmok()
        {
            Result = (Int16)ERROR_CODE.NONE
        };

        var body = MemoryPackSerializer.Serialize(resUserReady);
        var sendData = PacketMaker.MakePacket(PACKETID.RES_READY_OMOK, body);

        ServerNetwork.SendData(sessionID, sendData);
    }

    public bool IsGameStartPossible(Room room)
    {
        var roomUserList = room.GetUserList();

        if(roomUserList.Count < ServerNetwork.ServerOption.RoomMaxUserCount)
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

        var sendData = PacketMaker.MakePacket(PACKETID.NTF_START_OMOK, bodyData);
        ServerNetwork.MainLogger.Debug("Game Start...");

        room.Broadcast("", sendData);
        
        room.OmokBoard.StartGame();
        room.OmokBoard.SetPlayerColor(userList[randomBlackIndex].NetSessionID, userList[room.CurrentUserCount() - randomBlackIndex - 1].NetSessionID); ;
    }

    public void RequestPutOmok(PacketData packetData)
    {
        var sessionID = packetData.SessionID;
        ServerNetwork.MainLogger.Debug("돌 두기 요청");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqPutMok>(packetData.BodyData);
            var turnPlayer = sessionID;
            var room = GetRoom(reqData.RoomNumber);

            //게임 아직 시작하지 않았다면 돌려보내기(NOT STARTED)
            if (room.OmokBoard.GameFinish == true)
            {
                ResponsePutOmok(ERROR_CODE.OMOK_NOT_STARTED, turnPlayer);
                return;
            }

            //만약 올바른 턴 유저가 보낸 것이 아니라면 돌려보내기
            if (room.OmokBoard.WhoseTurn() != turnPlayer)
            {
                ResponsePutOmok(ERROR_CODE.OMOK_TURN_NOT_MATCH, turnPlayer);
                return;
            }
            
            //putomok으로 이 자리에 돌을 둘 수 있는지 확인
            //true면 서버에서 돌 두기, 클라에 돌 둘 수 있다고 반환(None)
            //false면 서버에서 돌 두지 않고 클라에 리스폰스(ALREADY EXIST)

            if(room.OmokBoard.CheckAvailablePosition(reqData.PosX, reqData.PosY)==false)
            {
                ResponsePutOmok(ERROR_CODE.OMOK_ALREADY_EXIST, turnPlayer);
            }

            var putStone = room.OmokBoard.PutStone(reqData.PosX, reqData.PosY);

            //response message 보내서 돌 두기
            ResponsePutOmok(ERROR_CODE.NONE, turnPlayer);

            var notifyPutOmok = new PKTNtfPutMok()
            {
                PosX = reqData.PosX,
                PosY = reqData.PosY,
                Mok = room.OmokBoard.CurTurnCount
            };

            var body = MemoryPackSerializer.Serialize(notifyPutOmok);
            var sendData = PacketMaker.MakePacket(PACKETID.NTF_PUT_OMOK, body);

            room.Broadcast("", sendData);

            //승리조건 판별
            //승리했다면 Notify하고 EndGame
            //만약 CurTurnCount가 19*19라면 draw
            if (room.OmokBoard.CheckWinningCondition(reqData.PosX,reqData.PosY)==true)
            {
                NotifyEndOmok(room, sessionID);
            }
            else if (room.OmokBoard.CurTurnCount >= 19 * 19)
            {
                //draw
                NotifyEndOmok(room, "");
            }
            else
            {
                ServerNetwork.MainLogger.Debug("게임 계속 진행...");
            }
        }
        catch (Exception ex) 
        {
            ServerNetwork.MainLogger.Error(ex.ToString());
        }
    }

    public void ResponsePutOmok(ERROR_CODE errorCode, string sessionID)
    {
        var resPutOmok = new PKTResPutMok()
        {
            Result = (Int16)errorCode
        };

        var body = MemoryPackSerializer.Serialize(resPutOmok);
        var sendData = PacketMaker.MakePacket(PACKETID.RES_PUT_OMOK, body);

        ServerNetwork.SendData(sessionID, sendData);
    }

    public void NotifyEndOmok(Room room, string sessionID)
    {
        room.SetAllInitState();

        var ntfEndOmok = new PKTNtfEndOmok();

        if (sessionID != "")
        {
            var winner = room.GetUserByNetSessionID(sessionID);
            ntfEndOmok.WinUserID = winner.UserID;
        }
        else
        {
            ntfEndOmok.WinUserID = "DRAW";
        }
        
        var body = MemoryPackSerializer.Serialize(ntfEndOmok);
        var sendData = PacketMaker.MakePacket(PACKETID.NTF_END_OMOK, body);

        room.Broadcast("", sendData);
    }
}
