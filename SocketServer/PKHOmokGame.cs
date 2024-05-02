using CSBaseLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class PKHOmokGame:PKHandler
{
    PacketToBytes PacketMaker = new PacketToBytes();

    public void RegisterPacketHandler(Dictionary<int, Action<PacketData>> packetHandlerMap)
    {

    }



}
