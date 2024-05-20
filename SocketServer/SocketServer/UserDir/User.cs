using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketLibrary;

namespace SocketServer.UserDir;

public class User
{
    //??무슨 의미의 변수???
    ulong SequenceNumber = 0;
    //매번 바뀌는 Unique한 값.
    //일반적으로 패킷 순서 추적, 특정 작업/이벤트의 순서 추적하는 데 사용되는 고유값
    //이를 통해 데이터가 올바른 순서로 처리되고 중복, 누락 없이 전달됨
    string SessionID;
    //supersocket이 sessionID를 보고 req, res, ntf 할 수 있음

    string UserID;
    public int RoomNumber { get; set; } = -1;

    bool Connection = false;
    private int TimeSpan;
    public DateTime LastHeartbeat { get; set; }
    public DateTime ConnectedTime { get; set; }

    public void InitTimeSpan(int timeSpan)
    {
        TimeSpan = timeSpan;
    }

    public void Set(ulong sequence, string sessionID, string userID)
    {
        SequenceNumber = sequence;
        SessionID = sessionID;
        UserID = userID;
    }

    public ErrorCode LoginUpdateID(string userID)
    {
        UserID = userID;
        return ErrorCode.None;
    }

    public bool CheckHeartBeat(DateTime curTime)
    {
        var diff = curTime - LastHeartbeat;

        if ((int)diff.TotalMilliseconds > TimeSpan)
        {
            return false;
        }

        return true;
    }

    public bool CheckInactiveLogin(DateTime curTime)
    {
        if (UserID != "")
        {
            return true;
        }

        var diff = curTime - ConnectedTime;

        if ((int)diff.TotalMilliseconds > TimeSpan)
        {
            return false;
        }

        return true;
    }

    public bool IsSessionConfirm(string netSessionID)
    //서버가 연결 시도하는 세션이 이 세션이 맞는가?
    {
        return SessionID == netSessionID;
    }

    public string ID()
    {
        return UserID;
    }

    public string UserSessionID()
    {
        return SessionID;
    }

    public void RoomEnter(int roomNumber)
    {
        RoomNumber = roomNumber;
    }

    public void UpdateHeartbeat(DateTime now)
    {
        LastHeartbeat = now;
    }

    public void LeaveRoom()
    {
        RoomNumber = -1;
    }

    public bool IsLoginState()
    {
        return SequenceNumber != 0;
    }

    public bool IsInRoom()
    {
        return RoomNumber != -1;
    }

    public bool IsUserConnecting()
    {
        return Connection;
    }

    public void StartConnecting()
    {
        Connection = true;
    }

    public void EndConnecting()
    {
        Connection = false;
    }
}

