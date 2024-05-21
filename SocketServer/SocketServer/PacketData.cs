using SocketLibrary;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

//MainServer에서 onReceived해서 byte array 받으면
//PacketData대로 패킷화+deserialize
public class PacketData
{
    public Int16 PacketSize;
    public Int16 PacketID;
    public byte[] BodyData;

    public string SessionID;
    //패킷 전송자 식별하기 위한 ID(SuperSocketLite AppSession에서 자동지정)

    //inner
    public void Assign(Int16 packetID, byte[] packetBodyData)
    {
        SessionID = "";
        PacketID = packetID;

        if (packetBodyData != null)
        {
            BodyData = packetBodyData;
        }
    }

    public void Assign(string sessionID, Int16 packetID, byte[] packetBodyData)
    {
        SessionID = sessionID;
        PacketID = packetID;

        if(packetBodyData != null )
        {
            BodyData = packetBodyData;
        }
    }

    //클라와 연결된 순간 자기 자신에게 연결되었다고 알림(Notify)
    public PacketData MakeNTFInConnectOrDisconnectClientPacket(bool isConnect, string sessionID)
    {
        var packet = new PacketData();

        if (isConnect)//Connect
        {
            packet.PacketID = (Int16)PACKETID.NtfInConnectClient;
        }
        else//Disconnect
        {
            packet.PacketID = (Int16)PACKETID.NtfInDisconnectClient;
        }

        packet.SessionID = sessionID;//이 클라가 접속했다 알리기 위함
        return packet;
    }
}

//Internal Packet(시스템 내부 패킷, 자기 자신에게 보내는 패킷) 정의

[MemoryPackable]
public partial class PKTInternalReqRoomEnter
{
    public string UserID {  get; set; }
    public int RoomNumber {  get; set; }
}

[MemoryPackable]
public partial class PKTInternalResRoomEnter
{
    public ErrorCode Result { get; set; }
    public string UserID {  get; set; }
    public int RoomNumber { get; set;}
}

[MemoryPackable]
public partial class PKTInternalNtfRoomLeave
{
    public string UserID { get; set; }
    public int RoomNumber { get; set; }
}
