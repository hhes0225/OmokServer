using CSBaseLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;
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
