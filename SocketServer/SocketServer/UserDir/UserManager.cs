using SocketLibrary;
using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SocketServer.UserDir;

public class UserManager
{
    int MaxUserCount;//한 서버에 최대 유저 몇명?
    ulong UserSequenceNumber = 0;

    Dictionary<string, User> UserMap = new Dictionary<string, User>();
    
    List<User> UserArray = null;

    private Timer _checkConnectionTimer;

    public static Action<PacketData> SendInternalFunc;
    public static Action<string> CloseConnection;

    public ILog UserMgrLogger;


    public UserManager(ILog logger)
    {
        UserMgrLogger = logger;
    }

    public void Init(int maxUserCount, int startTime, int interval)
    {
        MaxUserCount = maxUserCount;
        UserArray = new List<User>();

        for (int i = 0; i < MaxUserCount; i++)
        {
            UserArray.Add(new User());
        }

        InitAndStartCheckTimer(startTime, interval);
    }


    //로그인 성공했을 시에만 UserManager에 등록해서 관리할 필요가 있음
    public ErrorCode AddUser(string userID, string sessionID)
    {
        if (IsFullUserCount())
        {
            return ErrorCode.LoginFullUserCount;
        }

        //sessionID로 이미 로그인 중인지 체크
        if (UserMap.ContainsKey(sessionID))
        {
            return ErrorCode.AddUserDuplication;
        }

        ++UserSequenceNumber;

        //유저를 유저 리스트에 등록
        int newIndex = GetUserListAvailableIndex();
        if (newIndex < 0)
        {
            return ErrorCode.LoginFullUserCount;
        }
        UserArray[newIndex].InitTimeSpan(10000);

        UserArray[newIndex].Set(UserSequenceNumber, sessionID, userID);
        UserArray[newIndex].StartConnecting();
        UserArray[newIndex].ConnectedTime = DateTime.Now;

        UserMap.Add(sessionID, UserArray[newIndex]);

        return ErrorCode.None;
    }

    public ErrorCode RemoveUser(string sessionID)
    {
        if (UserMap.Remove(sessionID) == false)
        {
            return ErrorCode.RemoveUserSearchFailureUserId;
        }

        foreach (var user in UserArray)
        {
            if (user.UserSessionID() == sessionID)
            {
                user.EndConnecting();
            }
        }

        return ErrorCode.None;
    }

    bool IsFullUserCount()
    {
        return UserMap.Count() >= MaxUserCount;
    }

    //유저 배열 or 딕셔너리에서 유저 세션과 인풋세션 비교, 같으면 리턴
    public User GetUserBySessionID(string netSessionID)
    {
        return UserArray.FirstOrDefault(x => x.UserSessionID() == netSessionID);
    }

    public int GetUserListAvailableIndex()
    {
        for (int i = 0; i < MaxUserCount; i++)
        {
            if (UserArray[i].IsUserConnecting() == false)
            {
                return i;
            }
        }

        return -1;
    }

    void InitAndStartCheckTimer(int startTime, int interval)
    {
        TimerCallback callback = new TimerCallback(SendUserConnectionCheck);
        _checkConnectionTimer = new Timer(callback, null, startTime, interval);
    }

    void StopCheckTimer()
    {
        _checkConnectionTimer.Dispose();
    }

    //250ms마다 호출, 노티파이 패킷 전송
    public void SendUserConnectionCheck(object state)
    {
        var ntfPkt = new PKTNtfInUserCheck();

        var body = MemoryPackSerializer.Serialize(ntfPkt);

        var internalPacket = new PacketData();
        internalPacket.Assign((int)PACKETID.NtfInUserCheck, body);

        SendInternalFunc(internalPacket);
    }

    public void CheckHeartBeat(int beginIndex, int endIndex)
    {
        if (endIndex >= MaxUserCount)
        {
            endIndex = MaxUserCount;
        }

        var curTime = DateTime.Now;


        for (int i = beginIndex; i < endIndex; i++)
        {
            //유저가 존재하는데 하트비트 안주고 있 접속 해제
            if (UserArray[i].IsUserConnecting() == true &&
                (UserArray[i].CheckHeartBeat(curTime) == false || UserArray[i].CheckInactiveLogin(curTime) == false))
            {
                CloseConnection(UserArray[i].UserSessionID());

                //유저 삭제
                UserArray[i].EndConnecting();
            }
        }

    }

    public int GetMaxUserCount()
    {
        return MaxUserCount;
    }
}


