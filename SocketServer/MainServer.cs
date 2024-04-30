using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class MainServer:AppServer<NetworkSession, OmokBinaryRequestInfo>
{
    public ServerOption ServerOption = new ServerOption();
}

public class NetworkSession:AppSession<NetworkSession, OmokBinaryRequestInfo>
{
}