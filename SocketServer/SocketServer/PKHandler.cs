using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSBaseLib;

namespace SocketServer;

public class PKHandler
{
    //protected MainServer ServerNetwork;
    //SendData
    //Distrubute
    public static Func<string, byte[], bool> SendDataFunc;
    public static Action<PacketData> DistributeFunc;

    //MainLogger
    protected SuperSocket.SocketBase.Logging.ILog HandlerLogger;

    protected UserManager _userMgr = null;

    public void Init(SuperSocket.SocketBase.Logging.ILog logger, UserManager userMgr)
    {
        HandlerLogger = logger;
        _userMgr=userMgr;
    }
}
