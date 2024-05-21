using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLibrary;

//client와 server간 서로 주고받을 Packet 형식, 패킷화, 디패킷화에 대해 정의
//이 코드는 cli-srv 같은 코드 공유해야 함

//패킷 데이터 관련 상수 정의(ParscalCase)
public class PacketDef
{
    public const Int16 PacketHeaderSize = 4;
    public const int MaxUserIDByteLength = 16;
    public const int MaxUserPWByteLength = 16;
    public const int InvalidRoomNumber = -1;
}

public class PacketToBytes
{
    public byte[] MakePacket(PACKETID packetID, byte[] bodyData)
    {
        var pktID = (Int16)packetID;
        Int16 bodyDataSize = 0;

        //패킷 헤더의 'PacketSize'(총 패킷 사이즈, 2 Byte Size) 계산하기 위한 부분
        if (bodyData != null)
        {
            bodyDataSize = (Int16)bodyData.Length;
        }

        var packetSize = (Int16)(PacketDef.PacketHeaderSize + bodyDataSize);

        //header + body 합친 완전체 패킷 만들기
        var completePacket = new byte[packetSize];

        //완전체 패킷에 header 데이터 삽입
        Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, completePacket, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, completePacket, 2, 2);

        if (bodyData != null)
        {
            Buffer.BlockCopy(bodyData, 0, completePacket, PacketDef.PacketHeaderSize, bodyDataSize);
        }

        return completePacket;
    }

    public static Tuple<int, byte[]> SplitBodyFromReceiveData(int recvLength, byte[] recvData)
    {
        var packetSize = BitConverter.ToInt16(recvData, 0);//header에서 읽어오기
        var packetID = BitConverter.ToInt16(recvData, 2);
        var bodySize = packetSize - PacketDef.PacketHeaderSize;

        var packetBody = new byte[bodySize];
        Buffer.BlockCopy(recvData, PacketDef.PacketHeaderSize, packetBody, 0, bodySize);
        //recvData의 offset header부터, packetbody의 0에 넣는다. bodysize만큼

        return new Tuple<int, byte[]>(packetID, packetBody);
    }
}
