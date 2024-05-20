using SocketLibrary;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketServer.UserDir;

namespace SocketServer.RoomDir;


public partial class Room
{
    public bool IsAllReady()
    {
        foreach (var user in UserList)
        {
            if (user.GetUserState() != UserState.Ready)
            {
                return false;
            }
        }

        return true;
    }

    public void StartGame()
    {
        GameStartTime = DateTime.Now;
        OmokBoard.StartGame();
    }

    public void EndGame()
    {
        FirstEntryTime = DateTime.Now;
        GameStartTime = DateTime.MinValue;
        OmokBoard.EndGame();
    }

    public void SetAllInitGameState()
    {
        foreach (var user in UserList)
        {
            user.InitState();
        }
    }

    public void NotifyPacketUserReady(string userID)
    {
        if (CurrentUserCount() == 0)
        {
            return;
        }

        var packet = new SocketLibrary.PKTNtfReadyOmok();
        packet.UserID = userID;
        packet.IsReady = (Int16)(GetUser(userID).GetUserState());

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NtfReadyOmok, bodyData);

        //방 전체에게 뿌리기 -> Broadcast
        Broadcast("", sendPacket);
    }

    public void NotifyClientTurnPass()
    {
        OmokBoard.PassTurn();

        var packet = new SocketLibrary.PKTNtfTurnPass();

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NtfTurnPass, bodyData);

        //방 전체에게 뿌리기 -> Broadcast
        Broadcast("", sendPacket);
    }

    public void NotifyEndOmok(string sessionID)
    {
        SetAllInitGameState();
        EndGame();

        var ntfEndOmok = new PKTNtfEndOmok();

        if (sessionID != "")
        {
            var winner = GetUserByNetSessionID(sessionID);
            ntfEndOmok.WinUserID = winner.UserID;

            var loser = GetLoser(winner.UserID);

            if (loser != null)
            {
                var data = new PKTNtfInGameResultUpdate()
                {
                    IsDraw = false,
                    Winner = winner.UserID,
                    Loser = loser.UserID
                };

                var internalBody = MemoryPackSerializer.Serialize(data);
                var internalPacket = new PacketData();
                internalPacket.Assign((short)PACKETID.NtfInGameResultUpdate, internalBody);

                SendDbInternalFunc(internalPacket);
            }
        }
        else
        {
            ntfEndOmok.WinUserID = OmokBoard.DrawGame();

            var data = new PKTNtfInGameResultUpdate()
            {
                IsDraw = true,
                Winner = UserList[0].UserID,
                Loser = UserList[1].UserID
            };

            var internalBody = MemoryPackSerializer.Serialize(data);
            var internalPacket = new PacketData();
            internalPacket.Assign((short)PACKETID.NtfInGameResultUpdate, internalBody);

            SendDbInternalFunc(internalPacket);
        }

        var body = MemoryPackSerializer.Serialize(ntfEndOmok);
        var sendData = PacketMaker.MakePacket(PACKETID.NtfEndOmok, body);

        Broadcast("", sendData);
    }

    public RoomUser GetLoser(string winner)
    {
        foreach (var user in UserList)
        {
            if (user.UserID == winner)
            {
                continue;
            }

            return user;
        }

        return null;
    }
}
