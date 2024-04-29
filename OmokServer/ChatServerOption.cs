using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer;


//echoServer보다 옵션이 많이 필요하기 때문에 하나의 파일 생성 

public class ChatServerOption
{
    [Option("uniqueID", Required = true, HelpText ="Server Unique ID")]
    public int ChatServerUniqueID {  get; set; }

    [Option("name", Required = true, HelpText ="Server Name")]
    public string Name { get; set; }


    // SuperSocket Library 에서 사용할 옵션들
    [Option("maxConnectionNumber", Required = true, HelpText = "MaxConnectionNumber")]
    public int MaxConnectionNumber {  get; set; }

    [Option("port", Required = true, HelpText ="Port")]
    public int Port { get; set; }

    // SuperSocket Library에서 클라이언트가 보낼 수 있는 패킷의 최대 크기
    // 이 크기 이상의 패킷이 전송되면 서버는 무시
    [Option("maxRequestLength", Required = true, HelpText ="maxRequestLength")]
    public int MaxRequestLength {  get; set; }

    // SuperSocket 리시브 버퍼 size
    // 메모리 할당 최소화하기 위해 하나의 큰 버퍼 만들고 이 버퍼에서 데이터 받고 받은 데이터를 application layer에 전달
    // 데이터 받을 때마다 버퍼 할당해줄 필요 없음
    [Option("receiveBufferSize", Required =true, HelpText ="receiveBufferSize")]
    public int ReceiveBufferSize {  get; set; }

    [Option("sendBufferSize", Required = true, HelpText ="sendBufferSize")]
    public int SendBufferSize { get; set; }


    //채팅 서버에서 사용할 옵션들
    //방 최대 몇개?
    [Option("roomMaxCount", Required = true, HelpText = "Max room count")]
    public int RoomMaxCount { get; set; } = 0;

    //한 방에 몇명의 유저 입장 가능?
    [Option("roomMaxUserCount", Required = true, HelpText = "RoomMaxUserCount")]
    public int RoomMaxUserCount { get; set; } = 0;

    //방 시작 번호?
    //게임 서버 늘어나면 방 번호 서로 겹치게 될 수 있음.
    //따라서 roomMaxCount, roomStartNumber를 통해 관리
    //ex) 서버 1은 0번부터 10개의 방 관리, 서버2는 10번부터 10개의 방 관리 ...
    [Option("roomStartNumber", Required = true, HelpText = "RoomStartNumber")]
    public int RoomStartNumber { get; set; } = 0;
}
