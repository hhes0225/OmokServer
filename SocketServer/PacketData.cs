using CSBaseLib;
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
            packet.PacketID = (Int16)PACKETID.NTF_IN_CONNECT_CLIENT;
        }
        else//Disconnect
        {
            packet.PacketID = (Int16)PACKETID.NTF_IN_DISCONNECT_CLIENT;
        }

        packet.SessionID = sessionID;//이 클라가 접속했다 알리기 위함
        return packet;
    }
}

//Internal Packet(시스템 내부 패킷, 자기 자신에게 보내는 패킷) 정의

[MemoryPackable]
public partial class PKTInternalReqRoomEnter
    //Room 입장 시 시스템 내부에 유저 ID, 방 번호 자기 자신에게 req(입장 가능 여부 체크..?)
{
    public string UserID {  get; set; }
    public int RoomNumber {  get; set; }
}

[MemoryPackable]
public partial class PKTInternalResRoomEnter
    //시스템 내부에서 방 입장 결과 전달
    //(결과, 누가, 어떤 방에 입장 시도했는지)
{
    public ERROR_CODE ErrorCode { get; set; }
    public string UserID {  get; set; }
    public int RoomNumber { get; set;}
}

[MemoryPackable]
public partial class PKTInternalNtfRoomLeave
    //시스템 내부에서 방 나갔을 때 전달
    //누가, 어떤 방에서 나갔는지
{
    public string UserID { get; set; }
    public int RoomNumber { get; set; }
}