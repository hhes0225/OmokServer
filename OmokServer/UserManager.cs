using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer;

using CSBaseLib;

//chat server는 유저 관리 필요 ex- 로그인한 유저만 방 입장 가능

public class UserManager
{
    int MaxUserCount;
    UInt64 UserSequenceNumber = 0;

    Dictionary<string, User> UserMap=new Dictionary<string, User>();    

    public void Init(int maxUserCount)
    {
        MaxUserCount = maxUserCount;
    }

    //sessionID: 한 클라이언트가 서버 요청한 순간부터 로그아웃 등으로 끊어지기까지 고유하게 유지됨.
    //userID가 기준이 아닌 이유: 한 클라이언트가 여러 세션 생성할 수 있기 때문(다중창 등...)
    //따라서 sessionID는 고유하지만 userID는 고유하지 않을 수도 있다.


    //로그인 성공했을 시에만 유저로 인식하고 관리
    public ERROR_CODE AddUser(string userID, string sessionID)
    {
        if (IsFullUserCount())
        {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }


        //sessionID는 유효 ID로 만들어지기 때문에 중복이 안됨
        //중복 체크
        if(UserMap.ContainsKey(sessionID)) {
            return ERROR_CODE.ADD_USER_DUPLICATION;
        }

        ++UserSequenceNumber;

        var user=new User();
        user.Set(UserSequenceNumber, sessionID, userID);//Set 구현 필요
        UserMap.Add(sessionID, user);

        return ERROR_CODE.NONE;
    }

    public ERROR_CODE RemoveUser(string sessionID)
    {
        if (UserMap.Remove(sessionID) == false)//여기서 유저 삭제됨
        {
            return ERROR_CODE.REMOVE_USER_SEARCH_FAILURE_USER_ID;
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
        return MaxUserCount <= UserMap.Count();
    }
}

public class User
{
    UInt64 SequenceNumber = 0; // 매번 바뀌는 unique한 값.
    //Uint64이기 때문에 몇십년 쌓이기만 해도 끄떡없음
    string SessionID; 
    //supersocket이 sessionID를 보고 어떤 session 객체에게 send해야 하는지 알 수 있음

    public int RoomNumber { get; private set; } = -1;
    string UserID;

    public void Set(UInt64 sequence, string sessionID, string userID)
    {
        SequenceNumber = sequence;
        SessionID = sessionID;
        UserID = userID;
    }

    public bool IsConfirm(string netSessionID)
    {
        return netSessionID == netSessionID;
    }

    public string ID()
    {
        return UserID;
    }

    public void EnteredRoom(int roomNumber)
    {
        RoomNumber=roomNumber;
    }

    public void LeaveRoom()
    {

        RoomNumber = -1;
    }

    public bool IsStatLogin() { return SequenceNumber != 0; }

    public bool IsStateRoom() { return RoomNumber != 1; }
}
