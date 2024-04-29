using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer;

//requstMessage 형식 규칙 지정
public class EFBinaryRequestInfo : BinaryRequestInfo
{
    //.패킷 헤더용 변수
    public Int16 Size {  get; private set; }//패킷의 총 길이, 2Byte
    public Int16 PacketID { get; private set; }//패킷 ID(어떤 요청인지), 2Byte;
    public SByte Type {  get; private set; }//Sbyte: 1byte 정수

    public EFBinaryRequestInfo(Int16 totalSize, Int16 packetID, sbyte type, byte[] body)
        :base(null, body)
    {
        Size = totalSize;
        PacketID = packetID;
        Type = type;
    }
}

//ReceiveFilter: EFBinaryRequesInfo 규칙에 따라 Packet 정보 분석
public class ReceiveFilter:FixedHeaderReceiveFilter<EFBinaryRequestInfo>
{
    //헤더 정보만 읽어옴(HEADER_SIZE에 의해 5byte)
    //CSBaseLib 네임스페이스 구현 필요(PacketData, PacketDefine)
    public ReceiveFilter() : base(CSBaseLib.PacketDef.PACKET_HEADER_SIZE)
    {
    }

    //필수 구현 클래스: Header 와 body 분리, body 크기 리턴
    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if(!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, 2);
        }

        var packetSize = BitConverter.ToInt16(header, offset);
        var bodySize= packetSize-CSBaseLib.PacketDef.PACKET_HEADER_SIZE;

        return bodySize;
    }

    //필수 구현 클래스: 리퀘

    protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(header.Array, offset, CSBaseLib.PacketDef.PACKET_HEADER_SIZE);
        }

        return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
            BitConverter.ToInt16(header.Array, 0 + 2),
            (sbyte)header.Array[4],
            bodyBuffer.CloneRange(offset, length)); 
        // 이 리턴값은 onReceived로 전달됨

    }
}
