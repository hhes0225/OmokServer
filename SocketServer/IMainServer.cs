using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public interface IMainServer
{
    SuperSocket.SocketBase.Logging.ILog MainLogger { get; set; }
}
