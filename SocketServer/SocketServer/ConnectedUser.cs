using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

class ConnectedUser
{
    public string SessionID = "";
    public DateTime ConnectedTime;
    public const int TimeSpan = 10000; // n초간 로그인 안되면 접속해제

    public bool IsUserConnecting()
    {
        if (SessionID == "")
        {
            return false;
        }

        return true;
    }

    public bool IsInactiveLogin(DateTime curTime)
    {
        var diff = curTime - ConnectedTime;

        if ((int)diff.TotalMilliseconds > TimeSpan)
        {
            return true;
        }

        return false;
    }
}
