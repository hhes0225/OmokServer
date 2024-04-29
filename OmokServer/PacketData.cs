using ChatServer;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSBaseLib;

//client와 server간 서로 주고받을 Packet에 대한 것 정의
//이 코드는 클라이언트와 같은 코드를 공유함.

public class PacketDef
{
    public const Int16 PACKET_HEADER_SIZE = 5;
    public const int MAX_USER_ID_BYTE_LENGTH = 16;
    public const int MAX_USER_PW_BYTE_LENGTH = 16;

    public const int INVALID_ROOM_NUMBER = -1;
}


//
public class PacketToBytes
{
    public static byte[] Make(PACKETID packetID, byte[] bodyData)
    {
        byte type = 0;
        var pktID = (Int16)packetID;
        Int16 bodyDataSize = 0;

        if(bodyData != null)
        {
            bodyDataSize= (Int16)bodyData.Length;
        }

        var packetSize=(Int16)(bodyDataSize+PacketDef.PACKET_HEADER_SIZE);//total packet size

        var dataSource = new byte[packetSize];
        Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);//total size
        //packetSize를 dataSource에 복사(dataSource의 0~1 index)
        Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);//packet ID
        //packetSize를 dataSource에 복사(dataSource의 2~3 index)
        dataSource[4] = type;//Type

        if(bodyData!= null)
        {
            Buffer.BlockCopy(bodyData, 0, dataSource, 5, bodyDataSize);
            /*
             
            bodyData: 복사할 데이터의 원본 배열
            0: 원본 배열에서 복사를 시작할 위치 (오프셋)
            ->bodyData의 처음부터 복사

            dataSource: 복사한 데이터를 저장할 대상 배열
            5: 대상 배열에서 복사를 시작할 위치 (오프셋)
            ->저장되는 배열은 dataSource, 헤더가 이미 저장되어 있으므로 5번째 인덱스부터 복사값 저장

            bodyDataSize: 복사할 바이트 수
            ->원본 데이터 길이만큼 복사
            */
        }

        return dataSource;
    }

    public static Tuple<int, byte[]> ClientReceiveData(int recvLength, byte[] recvData)
    {
        //Int16 = 2Byte 이므로 startIndex에서 2Byte를 가져와서 int16형으로 바꾼다.
        var packetSize = BitConverter.ToInt16(recvData, 0);
        var packetID = BitConverter.ToInt16(recvData, 2);
        var bodySize = packetSize - PacketDef.PACKET_HEADER_SIZE;

        var packetBody = new byte[bodySize];
        Buffer.BlockCopy(recvData, PacketDef.PACKET_HEADER_SIZE, packetBody, 0, bodySize);
        //recvData 배열에서 패킷헤더 사이즈 인덱스부터 bodySize만큼, packetBody로 첫번째 인덱스부터 복사

        return new Tuple<int, byte[]>(packetID, packetBody);
    }
}

//이하 내용은 !!!!!!Packet의 body에 해당하는 부분임!!!!!!!!!!

//로그인 기능
//요청: 유저 ID와 HiveServer에서 생성한 인증토큰을 전송
//(클라이언트가 이것을 게임 서버에 전송)
//응답: 게임서버는 게임Redis에서 클라이언트에게 받은 ID와 인증토큰 비교,
//(게임서버가 비교 결과를 클라이언트에게 전송)

[MemoryPackable]
public partial class  PKHeader
{
    public UInt16 TotalSize { get; set; } = 0;
    public UInt16 ID { get; set; } = 0;
    public byte Type { get; set; } = 0;
}

[MemoryPackable]
public partial class PKTReqLogin:PKHeader
{
    public string UserID { get; set; }
    public string AuthToken { get; set; }
}

[MemoryPackable]
public partial class PKTResLogin : PKHeader
{
    public short Result { get; set; }
}


//NTF는 'Notification’의 약어로 해석될 수 있으며, 클라이언트의 연결 상태 변경을 알리는 데 사용
[MemoryPackable]
public partial class PKNtfMustClose : PKHeader
{
    public short Result { get; set; }
}


//방 입장
//request: RoomNumber방에 들어가고 싶다고 요청
//response: RoomNumber 방에 들어갈 수 있는지 없는지 동작 후 결과 반환
[MemoryPackable]
public partial class PKTReqRoomEnter : PKHeader
{
    public int RoomNumber { get; set; }
}

[MemoryPackable]
public partial class PKTResRoomEnter : PKHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomUserList : PKHeader
{
    public List<string> UserIDList { get; set; } = new List<string>();
}

[MemoryPackable]
public partial class PKTNtfRoomNewUser : PKHeader
{
    public string UserID { get; set; }
}


//클라이언트가 방 나가기를 요청했을 때
[MemoryPackable]
public partial class PKTReqRoomLeave : PKHeader
{
}

[MemoryPackable]
public partial class PKTResRoomLeave : PKHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomLeaveUser : PKHeader
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTReqRoomChat : PKHeader
{
    public string ChatMessage { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomChat : PKHeader
{
    public string UserID { get; set; }
    public string ChatMessage { get; set; }
}
