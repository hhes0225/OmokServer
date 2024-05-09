using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class Room
{
    public int Index { get; set; }
    public int Number {  get; set; }
    public PacketToBytes PacketMaker = new PacketToBytes();

    public OmokRule OmokBoard = new OmokRule();

    int MaxUserCount = 0;

    List <RoomUser> UserList = new List<RoomUser> ();

    public static Func<string, byte[], bool> NetSendFunc;
    //MainServer에서 참조할 함수 지정
    public static Func<string, User> GetUserFromUserMgr;

    public bool IsRoomUsing = false;
    public DateTime FirstEntryTime { get; private set; }
    public DateTime GameStartTime {  get; private set; }

    private int RoomTimeSpan, GameTimeSpan, GameTurnTimeSpan;

    public void Init(int index, int number, int maxUserCount) 
    { 
        Index = index;
        Number = number;
        MaxUserCount = maxUserCount;

    }

    public void InitTimeSpan(int roomTimeSpan, int gameTimeSpan, int gameTurnTimeSpan)
    {
        RoomTimeSpan = roomTimeSpan;
        GameTimeSpan = gameTimeSpan;
        GameTurnTimeSpan = gameTurnTimeSpan;
    }

    public bool AddUser(string userID, string netSessionID)
    {
        if (GetUser(userID) != null)
        {
            return false;
        }

        var newRoomUser = new RoomUser();
        newRoomUser.Set(userID, netSessionID);
        UserList.Add(newRoomUser);

        ActivateRoom();

        return true;
    }

    public void ActivateRoom()
    {
        if (IsRoomUsing == false)
        {
            IsRoomUsing = true;
            FirstEntryTime = DateTime.Now;
        }
        
    }
    public void InactivateRoom()
    {
        if (IsRoomUsing == true)
        {
            IsRoomUsing = false;
            FirstEntryTime = DateTime.MinValue;
        }
    }

    public bool IsRoomCreatedButNotPlaying(DateTime curTime)
    {
        var diff = curTime- FirstEntryTime;

        //나중에 값 바꿀 것
        if((int)diff.TotalMinutes >= RoomTimeSpan && OmokBoard.GameFinish == true)
        {
            return false;
        }

        return true;
    }

    public bool IsGamePlayingTooLong(DateTime curTime)
    {
        var diff = curTime - GameStartTime;

        if((int) diff.TotalHours >= GameTimeSpan && OmokBoard.GameFinish == false) 
        {
            return false;
        }

        return true;

    }

    

    public void RemoveAllUser()
    {
        for(int i = 0; i < UserList.Count; i++)
        {
            if (UserList[i] != null)
            {
                NotifyPacketLeaveUser(UserList[i].UserID);
                Console.WriteLine($"{UserList[i].UserID}: {DateTime.Now}");

                var user = GetUserFromUserMgr(UserList[i].NetSessionID);

                if (user!=null)
                {
                    user.LeaveRoom();
                }
                
                RemoveUser(UserList[i]);
            }
        }
    }

    public bool RemoveUser(RoomUser user)
    {
        if(user == null)
        {
            return false;
        }

        var result = UserList.Remove(user);

        if (CurrentUserCount() == 0)
        {
            InactivateRoom();
        }

        return result;
    }

    public List<RoomUser> GetUserList()
    {
        return UserList;
    }

    public RoomUser GetUser(string userID)
    {
        return UserList.Find(x => x.UserID == userID);
    }

    public RoomUser GetUserByNetSessionID(string netSessionID)
    {
        return UserList.Find(x => x.NetSessionID == netSessionID);
    }

    public int CurrentUserCount()
    {
        return UserList.Count;
    }

    public void NotifyPacketUserList(string userNetSessionID)
    {
        var packet = new CSBaseLib.PKTNtfRoomUserList();

        foreach(var user in UserList)
        {
            packet.UserIDList.Add(user.UserID);
        }

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_ROOM_USER_LIST, bodyData);

        NetSendFunc(userNetSessionID, sendPacket);
    }

    public void NotifyPacketNewUser(string newUserNetSessionID, string newUserID)
    {
        var packet = new CSBaseLib.PKTNtfRoomNewUser();
        packet.UserID = newUserID;

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_ROOM_NEW_USER, bodyData);

        Broadcast(newUserNetSessionID, sendPacket);
    }

    public void NotifyPacketLeaveUser(string userID)
    {
        if(CurrentUserCount() == 0) 
        {
            return;
        }

        var packet = new CSBaseLib.PKTNtfRoomLeaveUser();
        packet.UserID = userID;

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_ROOM_LEAVE_USER, bodyData);

        //방 전체에게 뿌리기 -> Broadcast
        Broadcast("", sendPacket);
    }

    public void NotifyPacketUserReady(string userID)
    {
        if (CurrentUserCount() == 0)
        {
            return;
        }

        var packet = new CSBaseLib.PKTNtfReadyOmok();
        packet.UserID = userID;
        packet.IsReady = (Int16)(GetUser(userID).GetUserState());

        var bodyData = MemoryPackSerializer.Serialize(packet);
        var sendPacket = PacketMaker.MakePacket(PACKETID.NTF_READY_OMOK, bodyData);

        //방 전체에게 뿌리기 -> Broadcast
        Broadcast("", sendPacket);
    }

    public void NotifyEndOmok(string sessionID)
    {
        SetAllInitState();
        EndGame();

        var ntfEndOmok = new PKTNtfEndOmok();

        if (sessionID != "")
        {
            var winner = GetUserByNetSessionID(sessionID);
            ntfEndOmok.WinUserID = winner.UserID;
        }
        else
        {
            ntfEndOmok.WinUserID = OmokBoard.DrawGame();
        }

        var body = MemoryPackSerializer.Serialize(ntfEndOmok);
        var sendData = PacketMaker.MakePacket(PACKETID.NTF_END_OMOK, body);

        Broadcast("", sendData);
    }

    public void StartGame()
    {
        GameStartTime= DateTime.Now;
        OmokBoard.StartGame();
    }

    public void EndGame()
    {
        GameStartTime = DateTime.MinValue; 
        OmokBoard.EndGame();
    }

    public void Broadcast(string excludeNetSessionID, byte[] sendPacket)
    {
        foreach(var user in UserList)
        {
            //자기 자신을 제외한 방 안 모든 유저에게 전송
            if (user.NetSessionID == excludeNetSessionID)
            {
                continue;
            }

            NetSendFunc(user.NetSessionID, sendPacket);//SendData
        }
    }

    public bool IsAllReady()
    {
        foreach(var user in UserList)
        {
            if (user.GetUserState()!=UserState.Ready)
            {
                return false;
            }
        }

        return true;
    }

    public void SetAllInitState()
    {
        foreach (var user in UserList)
        {
            user.InitState();
        }
    }
}

public class RoomUser
{
    public string UserID { get; private set; }
    public string NetSessionID { get; private set; }
    public UserState State { get; private set; } = UserState.None;

    public void Set(string userID, string netSessionID)
    {
        UserID = userID;
        NetSessionID = netSessionID;
    }
    public UserState GetUserState()
    {
        return State;
    }

    public void Ready()
    {
        State = UserState.Ready;
    }

    public void Play()
    {
        State = UserState.Playing;
    }

    public void InitState()
    {
        State = UserState.None;
    }

}
