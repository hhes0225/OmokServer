using CSBaseLib;
using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    List<User> UserArray=null;

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
    public ERROR_CODE AddUser(string userID, string sessionID)
    {
        if (IsFullUserCount())
        {
            return ERROR_CODE.LoginFullUserCount;
        }

        //sessionID로 이미 로그인 중인지 체크
        if (UserMap.ContainsKey(sessionID))
        {
            return ERROR_CODE.AddUserDuplication;
        }

        ++UserSequenceNumber;

        //유저를 유저 리스트에 등록
        int newIndex = GetUserListAvailableIndex();
        if (newIndex < 0)
        {
            return ERROR_CODE.LoginFullUserCount;
        }
        UserArray[newIndex].InitTimeSpan(10000);

        UserArray[newIndex].Set(UserSequenceNumber, sessionID, userID);
        UserArray[newIndex].StartConnecting();
        UserArray[newIndex].ConnectedTime= DateTime.Now;

        UserMap.Add(sessionID, UserArray[newIndex]);

        return ERROR_CODE.None;
    }

    public ERROR_CODE RemoveUser(string  sessionID) 
    {
        if (UserMap.Remove(sessionID) == false)
        {
            return ERROR_CODE.RemoveUserSearchFailureUserId;
        }

        foreach(var user in UserArray)
        {
            if (user.UserSessionID() == sessionID)
            {
                user.EndConnecting();
            }
        }

        return ERROR_CODE.None;
    }

    bool IsFullUserCount()
    {
        return UserMap.Count()>=MaxUserCount;
    }

    //유저 배열 or 딕셔너리에서 유저 세션과 인풋세션 비교, 같으면 리턴
    public User GetUserBySessionID(string netSessionID)
    {
        return UserArray.FirstOrDefault(x => x.UserSessionID() == netSessionID);
    }

    public int GetUserListAvailableIndex()
    {
        for(int i=0;i<MaxUserCount;i++)
        {
            if (UserArray[i].IsUserConnecting()==false)
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
        
        
        for(int i = beginIndex; i < endIndex; i++)
        {
            //유저가 존재하는데 하트비트 안주고 있 접속 해제
            if ((UserArray[i].IsUserConnecting() == true)&&
                (UserArray[i].CheckHeartBeat(curTime)==false || UserArray[i].CheckInactiveLogin(curTime)==false)) 
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


