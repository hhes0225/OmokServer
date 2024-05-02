using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSBaseLib;

namespace SocketServer;

public class PKHandler
{
    //Q. 상호 참조 안되는지???
    protected MainServer ServerNetwork;
    protected UserManager UserMgr=null;

    public void Init(MainServer serverNetwork, UserManager userMgr)
    {
        ServerNetwork=serverNetwork;
        UserMgr=userMgr;
    }
}
