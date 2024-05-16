using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.SocketEngine.Protocol;
using SuperSocket.Common;

namespace SocketServer;

//request message를 받을 때, header와 body의 구조(크기)를 파악하기 위한 클래스

public class OmokBinaryRequestInfo:BinaryRequestInfo
{
    //패킷 헤더 지정 변수(이 변수들을 헤더에 포함되는 변수임)
    public Int16 Size { get;private set; }//패킷 총 길이, 2B
    public Int16 PacketID {  get; private set; }//패킷 ID(어떤 유형의 요청인지), 2B

    //header(Size, packetID)는 bin->int 변환해야 정보를 얻을 수 있음
    //body는 packetID에 따라 다르게 deserialize해야 하므로 byte 배열 그대로
    public OmokBinaryRequestInfo(Int16 size, Int16 packetID, byte[] body)
        :base(null, body)
    {
        Size=size;
        PacketID = packetID;
    }
}

//헤더 정보만 읽어옴(HEADER_SIZE에 의해 4byte), body는 그대로 byte 배열
public class ReceiveFilter : FixedHeaderReceiveFilter<OmokBinaryRequestInfo>
{
    public ReceiveFilter() : base(CSBaseLib.PacketDef.PacketHeaderSize)
    {
    }

    //필수 구현 클래스, body 크기 리턴
    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, 2);
        }

        var packetSize = BitConverter.ToInt16(header, offset);
        //offset부터 2byte 변환, 0~2byte는 패킷 전체 길이에 대한 정보 저장
        var bodySize = packetSize - CSBaseLib.PacketDef.PacketHeaderSize;

        return bodySize;
    }

    protected override OmokBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
    {
        return new OmokBinaryRequestInfo(
            BitConverter.ToInt16(header.Array, 0),
            BitConverter.ToInt16(header.Array, 0+2),
            bodyBuffer.CloneRange(offset, length)
            );
        //header, body 배열 각각 처음부터 시작하고 있으므로 이 경우 offset은 0
    }
}
