- 서버는 웬만해서 wait, sleep 넣으면 안됨(대신 Timer를 사용)
### UserManager.cs
```
//동접자 1000명, 250ms마다 하트비트 검사

private Time _checkTimer = new Timer();//유저 매니저에서 타이머
//private Int64 _startIndexUserCheck = 0;

List<User> _userList = new();//인덱스 번호로 접근하게 하기 위함
//잘못됨! List가 아니라 Array로, 빈 Array에 넣기(netsession)
//RoomManager처럼 가능한 유저 리스트 미리 생성
//Room은 list로 해도 됨(얘는 미리 다 방 생성해놓았기 때문)
//*** 딕셔너리 삽입 시 배열에 같이 삽입, 딕셔너리 제거 시 배열에 같이 제거 필요!!!!!!!
//유저클래스에 유저가 있는지 없는지 변수를 하나 더 둬서 없다면 유저 넣는 식으로

//사용하지 않는 인덱스 큐로 관리해도 됨
//아니면 그냥 뒤져도 됨.(1000명대는 그게 더 효율적)

public int AddToEmptyIndex(newUser){
	foreach(var user in _userList)
	{
		if(user._used==true){
			continue;
		}
		
		user=newUser;
	}
}

public void Init(int maxUSerCount){
	MaxUserCount=maxUsercount
	
	_checkTimer.Interval=250;
	_checkTimer.run(UserCheck);
	//유저체크 메서드
	//C# 타이머는 별도의 스레드로 만들어짐 - thread safe
}

void UserCheck()//250ms마다 호출
{
	//inner packet 전송
	var internalPacket = InnerPacketMaker.MakeNTFInnerUserCheckPacket();
	DistributeInnerPacket(internalPacket);
	//+ messagebuffer가 스레드 세이프하기때문에 이것도 스레드세이프
}

void CheckHeartBeat(int beginIndex, int endIndex)
//C++ 참고: begin은 포함, end는 포함 X [begin, end)
{
	//안전하게 하기 위해 유저 값 비교 필요
	if(endIndex>_maxUserCount){
		endInex=_maxUserCount;//4로 안나누어떨어질때 발생할 수 있는 에러가 있음
	}
	
	var currTime = 
	
	for(int i=beginIndex;i<endIndex;i++)
	{
		var result = _userList[i].CheckHeartBeat();	
		if(result == false){//하트비트 안도미
			//접속을 끊는다.
		}
		
	}
}
```


### User.cs
```
private Datetime _heartbeat;//유저 connection될때
//클라이언트가 패킷 주면 그때부터 1초 단위로 하트비트 보내
private int Timespan // 이건 처음에 초기화해야함

private bool _used = false;
//유저 커넥트, 디스커넥트할때 유저가 있는지 없는지 확인
//(위의 UserManager의 새로 생성한 배열. 하트비트에 유저 확인용)

Init(int timeSpan);//상수로 주지 않고 초기화 함수 생성(나중에 시간 바꿔야할수도 있으므로)

public bool CheckHeartBeat(DateTime curTime){
	var diff = curTime-_heartbeat; // 정한 시간보다 크면 false, 작으면 true
	//
	if (diff>Timespan){
      return false;
  }

  return true;
}
```


### PKHCommon.cs - innerpacket 전송하는 핸들러
```
//+ registerPacketHandler 메서드에 함수 핸들러 등록
//+ PacketDefine에서 패킷 아이디 등록

private int _StartIndexUserCheck=0;//시작지점
private const int MaxCheckUserCount=250;//한번 검사할때 몇명 검사할건지(상수)
// 단, 이것도 위의 User처럼 상수로 두지 않고 Init메서드로 초기화해도 된다.

public void NotifyInUserCheck(){
	var endIndex = _StartIndexUserCheck+MaxCheckUserCount;
	_userMgr.CheckHeartBeat(_startIndexUserCheck, endIndex);
	
	_StartIndexUserCheck+=MaxCheckUserCount;
	if(_startIndexUserCheck > = _userMgr.MaxUserCount){
		_startIndexUserCheck = 0;//인덱스 마지막으로 갔다면 다시 처음으로
	}
}
```


- Room도 비슷하게(방에서 쫓아내는것은 괜찮지만 Disconnect하게 되지는 않음)
- Room객체(유저의 턴 제한시간): Timer 2개(방만든시간, 턴 시작된 시간)
- 룸에 룸 유저 리스트를 만들어 - 이것을 UserManager의 userList와 같이 쓰면 안됨.
- 퇴장시키지는 않고, 승패만 관리(나중에는 블랙리스트 등록까지 갈 수 있음)
- 만약 너무 턴을 안둔다면 해당 유저를 Lose처리
- 정책적인 부분에 따라 달라질 수 있음
- 1명인데 계속 플레이 안하고 있으면 낭비→이 사람 둘건지 쫓아낼건지
