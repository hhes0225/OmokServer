using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;

namespace CSBaseLib;

//PacketData.cs
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

//패킷 body 클래스를 byte 배열로 변환
//(해당 서버에서 전송하는 패킷 만들기 위함)
public class PacketToBytes
{
    public byte[] MakePacket(PACKETID packetID, byte[] bodyData)
    {
        var pktID = (Int16)packetID;//2byte 패킷ID 나타내는 헤더
        Int16 bodyDataSize = 0;

        //패킷 헤더의 'PacketSize'(총 패킷 사이즈, 2 Byte Size) 계산하기 위한 부분
        if(bodyData != null)
        {
            bodyDataSize=(Int16)bodyData.Length;
        }

        var packetSize = (Int16)(PacketDef.PacketHeaderSize+bodyDataSize);

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

    //packetID와 body 데이터 분리(Deserialize 위해)
    public static Tuple<int, byte[]> SplitBodyFromReceiveData(int recvLength, byte[] recvData)
    {
        //header의 size, id는 2byte이므로 2byte로 변환
        var packetSize = BitConverter.ToInt16(recvData, 0);//header에서 읽어오기
        var packetID = BitConverter.ToInt16(recvData, 2);
        var bodySize = packetSize - PacketDef.PacketHeaderSize;

        //body는 deserialize하지 않고 byte 그대로 전달
        var packetBody = new byte[bodySize];
        Buffer.BlockCopy(recvData, PacketDef.PacketHeaderSize, packetBody, 0, bodySize);
        //recvData의 offset header부터, packetbody의 0에 넣는다. bodysize만큼

        return new Tuple<int, byte[]>(packetID, packetBody);
    }
}

//각 req, res 유형에 따라 필요한 class 정의
//(serialize, deserialize 하기 위함)
//따라서 client측에서 보내주는 req도 정의 필요
//body와 header 따로 만들어주기 때문에 body 클래스만 정의

[MemoryPackable]
public partial class PKTReqLogin//cli->srv 로그인 reqest 패킷
{
    public string UserID {  get; set; }
    public string AuthToken {  get; set; }
}

[MemoryPackable]
public partial class PKTResLogin//srv->cli 로그인 결과 response
{
    public short Result {  get; set; }
}

[MemoryPackable]
public partial class PKTNtfMustClose//Notify-로그인 실패한 경우 반드시 close해야 함을 명시..?
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomEnter//cli->srv 방 입장 request
{
    public int RoomNubmer {  get; set; }
}

[MemoryPackable]
public partial class PKTResRoomEnter//srv->cli 방 입장 결과 response
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomUserList//방 입장 시 방 안 유저에게 서로의 존재를 notify?
{
    public List<string> UserIDList { get; set; } = new List<string>();
}

[MemoryPackable]
public partial class PKTNtfRoomNewUser//방에 새 유저 입장 시 이미 방에 있는 사람들에게 Notify
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomLeave//cli->srv 방 퇴장 request
{
}

[MemoryPackable]
public partial class PKTResRoomLeave//srv->cli 방 퇴장 결과 response
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomLeaveUser//방 구성원에게 나간 유저 정보 Notify
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomChat//cli->srv 채팅 메시지 보내기 request
{
    public string ChatMessage {  get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomChat//Ntf == Response 역할을 하게 됨. 실행 결과 채팅 메시지 다른 사용자들에게 broadcast.
{
    public string UserID { get; set; }
    public string ChatMessage { get; set; }
}

[MemoryPackable]
public partial class PKTReqReadyOmok
{
    public int RoomNumber;
    public string UserID;
}

[MemoryPackable]
public partial class PKTResReadyOmok
{
    public short Result;
}

[MemoryPackable]
public partial class PKTNtfReadyOmok
{
    public string UserID;
    public short IsReady;
}

// 오목 시작 통보(서버에서 클라이언트들에게)
[MemoryPackable]
public partial class PKTNtfStartOmok
{
    public string BlackUserID; // 선턴 유저 ID
    public string WhiteUserID;
}

// 돌 두기
[MemoryPackable]
public partial class PKTReqPutMok
{
    public int RoomNumber;
    public int PosX;
    public int PosY;
}

[MemoryPackable]
public partial class PKTResPutMok
{
    public short Result;
}

[MemoryPackable]
public partial class PKTNtfPutMok
{
    public int PosX;
    public int PosY;
    public int Mok;
}

// 오목 게임 종료 통보
[MemoryPackable]
public partial class PKTNtfEndOmok
{
    public string WinUserID;
}