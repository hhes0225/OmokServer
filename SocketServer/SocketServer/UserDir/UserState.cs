using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.UserDir;

public enum UserState
{
    None = 0,
    Ready = 1,
    Playing = 2,
    Finished = 3
}
