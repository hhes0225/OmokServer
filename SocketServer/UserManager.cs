using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

//현재 로그인한 유저(전체유저 X) == 서버와 통신해서 sessionID가 있는 유저 관리
//sessionID: 한 클라이언트가 서버 요청한 순간부터 로그아웃 등으로 끊어지기까지 고유하게 유지됨.
//userID가 기준이 아닌 이유: 한 클라이언트가 여러 세션 생성할 수 있기 때문(다중창 등...)
//따라서 sessionID는 고유하지만 userID는 고유하지 않을 수도 있다.

public class UserManager
{
    int MaxUserCount;//한 서버에 최대 유저 몇명?
    UInt64 UserSequenceNumber = 0;

    Dictionary<string, User> UserMap = new Dictionary<string, User>();
    //key는 sessionID, value는 이에 맞는 User
    User[] UserArray=null;
    ConnectedUser[] ConnectedButInactiveUser;

    private Timer _checkConnectionTimer;

    public static Action<PacketData> SendInnerPacket;
    public static Action<string> CloseConnection;

    private int Timespan;


    public void Init(int maxUserCount, int startTime, int interval, int timespan)
    {
        MaxUserCount = maxUserCount;
        UserArray = new User[MaxUserCount];
        ConnectedButInactiveUser = new ConnectedUser[MaxUserCount];

        for (int i = 0; i < MaxUserCount; i++)
        {
            UserArray[i] = new User();
            ConnectedButInactiveUser[i] = new ConnectedUser();
        }

        Timespan = timespan;

        InitAndStartCheckTimer(startTime, interval);
    }

    public ERROR_CODE AddJustConnectedUser(string sessionID)
    {
        if (IsFullUserCount())
        {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }

        //sessionID로 이미 로그인 중인지 체크
        if (UserMap.ContainsKey(sessionID))
        {
            return ERROR_CODE.ADD_USER_DUPLICATION;
        }

        ConnectedUser user = new ConnectedUser();
        user.SessionID = sessionID;
        user.ConnectedTime = DateTime.Now;

        if (GetUserArrayAvailableIndex() < 0)
        {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }
        ConnectedButInactiveUser[GetUserArrayAvailableIndex()] = user;

        return ERROR_CODE.NONE;
    }

    //로그인 성공했을 시에만 UserManager에 등록해서 관리할 필요가 있음
    public ERROR_CODE AddUser(string userID, string sessionID)
    {
        if (IsFullUserCount())
        {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }

        //sessionID로 이미 로그인 중인지 체크
        if (UserMap.ContainsKey(sessionID))
        {
            return ERROR_CODE.ADD_USER_DUPLICATION;
        }

        ++UserSequenceNumber;

        //유저를 유저 리스트에 등록
        User user = new User();
        user.InitTimeSpan(Timespan);
        user.Set(UserSequenceNumber, sessionID, userID);
        user.StartConnecting();
        user.ActivatedTime= DateTime.Now;

        UserMap.Add(sessionID, user);

        if (GetUserArrayAvailableIndex() < 0) {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }
        UserArray[GetUserArrayAvailableIndex()] = user;

        return ERROR_CODE.NONE;
    }

    public ERROR_CODE RemoveUser(string  sessionID) 
    {
        if (UserMap.Remove(sessionID) == false)
        {
            return ERROR_CODE.REMOVE_USER_SEARCH_FAILURE_USER_ID;
        }

        foreach(var user in UserArray)
        {
            if (user.UserSessionID() == sessionID)
            {
                user.EndConnecting();
            }
        }

        return ERROR_CODE.NONE;
    }

    public User GetUser(string sessionID)
    {
        User user = null;
        UserMap.TryGetValue(sessionID, out user);
        return user;
    }

    bool IsFullUserCount()
    {
        return UserMap.Count()>=MaxUserCount;
    }

    //유저 배열 or 딕셔너리에서 유저 세션과 인풋세션 비교, 같으면 리턴
    public User GetUserByNetSessionID(string netSessionID)
    {
        return UserArray.FirstOrDefault(x => x.UserSessionID() == netSessionID);
    }

    public int GetUserArrayAvailableIndex()
    {
        for(int i=0;i<UserArray.Count();i++)
        {
            if (UserArray[i].IsUserConnecting()==false)
            {
                return i;
            }
        }

        return -1;
    }


    //이거 나중에 지울 것.
    public void JustCheckLog(string sessionID)
    {
        var arr = GetUserByNetSessionID(sessionID);
        var dict = UserMap[sessionID];

        Console.WriteLine($"arr: {arr.LastHeartbeat}");
        Console.WriteLine($"dict: {dict.LastHeartbeat}");
    }

    void InitAndStartCheckTimer(int startTime, int interval)
    {
        TimerCallback callback = new TimerCallback(SendUserCheckPkt);
        _checkConnectionTimer = new Timer(callback, null, startTime, interval);
    }

    void StopCheckTimer()
    {
        _checkConnectionTimer.Dispose();
    }

    //250ms마다 호출, 노티파이 패킷 전송
    public void SendUserCheckPkt(object state)
    {
        var ntfPkt = new PKTNtfInnerUserCheck();

        var body = MemoryPackSerializer.Serialize(ntfPkt);

        var internalPacket = new PacketData();
        internalPacket.Assign((int)PACKETID.NTF_INNER_USER_CHECK, body);

        SendInnerPacket(internalPacket);
    }

    public void CheckHeartBeat(int beginIndex, int endIndex)
    {
        if (endIndex > MaxUserCount)
        {
            endIndex = MaxUserCount;
        }

        var curTime = DateTime.Now;
        
        //유저 배열 순회
        for(int i = beginIndex; i < endIndex; i++)
        {
            //유저가 없을 때, 하트비트 잘 주고 있을 때는 아무 동작 X
            if (UserArray[i].IsUserConnecting() == false)
            {
                continue;
            }

            if (UserArray[i].CheckHeartBeat(curTime) == true)
            {
                continue;
            }

            //유저가 존재하는데 하트비트 안주고 있으면 접속 끊는다
            CloseConnection(UserArray[i].UserSessionID());
        }

        
    }

    public void DisconnectInactiveUser(int beginIndex, int endIndex)
    {
        if (endIndex > MaxUserCount)
        {
            endIndex = MaxUserCount;
        }

        var curTime = DateTime.Now;

        //로그인 하지 않고 접속만 한 유저 체크
        for (int i = beginIndex; i < endIndex; i++)
        {
            //유저가 없을 때, 하트비트 잘 주고 있을 때는 아무 동작 X
            if (ConnectedButInactiveUser[i].SessionID == "")
            {
                continue;
            }

            var connUser = new User();

            if (UserMap.TryGetValue(ConnectedButInactiveUser[i].SessionID, out connUser) == true)
            {
                if (ConnectedButInactiveUser[i].IsInactiveLogin(connUser.ActivatedTime) == true)
                {
                    ConnectedButInactiveUser[i] = new ConnectedUser();
                }
            }
            else
            {
                if (ConnectedButInactiveUser[i].IsInactiveLogin(curTime) == true)
                {
                    continue;
                }
            }

            //유저가 접속했지만 로그인 안하는 경우도 끊는다
            CloseConnection(ConnectedButInactiveUser[i].SessionID);
        }
    }

    public int GetMaxUserCount()
    {
        return MaxUserCount;
    }
}

class ConnectedUser
{
    public string SessionID="";
    public DateTime ConnectedTime;
    public const int TimeSpan = 10000; // n초간 로그인 안되면 접속해제

    public bool IsInactiveLogin(DateTime curTime)
    {
        var diff = curTime - ConnectedTime;

        if ((int)diff.TotalMilliseconds > TimeSpan)
        {
            return false;
        }

        return true;
    }
}
