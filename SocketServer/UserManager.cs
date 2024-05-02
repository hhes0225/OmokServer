using CSBaseLib;
using System;
using System.Collections.Generic;
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

    public void Init(int maxUserCount)
    {
        MaxUserCount = maxUserCount;
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
        user.Set(UserSequenceNumber, sessionID, userID);
        UserMap.Add(sessionID, user);

        return ERROR_CODE.NONE;
    }

    public ERROR_CODE RemoveUser(string  sessionID) 
    {
        if (UserMap.Remove(sessionID) == false)
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
        return UserMap.Count()>=MaxUserCount;
    }
}

