using CSBaseLib;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer;



public class ServerPacketData
{
    public Int16 PacketSize;
    public string SessionID;
    public Int16 PacketID;
    public sbyte Type;
    public byte[] BodyData;

    public void Assign(string sessionID, Int16 packetID, byte[] packetBodyData)
    {
        SessionID = sessionID;
        PacketID = packetID;

        if(packetBodyData.Length>0)
        {
            BodyData= packetBodyData;
        }
    }

    public static ServerPacketData MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
    {
        var packet = new ServerPacketData();

        if(isConnect)
        {
            packet.PacketID = (Int32)PACKETID.NTF_IN_CONNECT_CLIENT;
        }
        else
        {
            packet.PacketID = (Int32)PACKETID.NTF_IN_DISCONNECT_CLIENT;
        }

        packet.SessionID = sessionID;
        return packet;
    }
}

[MemoryPackable]
public partial class PKTInternalReqRoomEnter:PKHeader
{
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTInternalResRoomEnter : PKHeader
{
    public ERROR_CODE Result { get; set; }
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTInternalNtfRoomLeave : PKHeader
{
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
}