- 서버는 웬만해서 wait, sleep 넣으면 안됨(대신 Timer를 사용)


# Heart beat

## 유저 상태 체크

- O 클라이언트에서 Timer로 n초마다 유저 정보(아이디, 토큰) 담긴 패킷 전송
→ 로그인 버튼 클릭하고, 로그인 리스폰스 메시지 받는 순간부터 전송 시작
→ 로그인 리스폰스 핸들러 함수에 Timer.run(reqAuth)하기
→ 연결 끊기 버튼 누른 순간 Dispose(타이머 끊기)

- 서버는 이를 받아서 유저배열 정보 업데이트
→ 유저 요소 user class 에 connected true로 하고, 유저 접속 시간 저장

- 서버는 Timer로 일정 주기마다 서버의 유저 배열 정보 체크하고 n초 이상 유저 정보 업데이트가 안되고 있으면 유저 연결 끊기
    
    → User class 유저 요소에 connected = false면 체크하지 않음., true일 경우만 세부내용 체크(false면 유저 접속이 없다는 뜻임)
    

## 방 상태 체크

### 체크해야 할 사항

- 방 생성된지 얼마나 지났는지?(만약 방에 유저 한명이라도 있는데 그 상태로 1시간이 지나면(+게임 시작했는데 게임이 안끝나고 1시간이 지나면) 방에 있는 사람 내쫓고 새로운 사람들 들여보내기
    - 필요한 것:
    - 클라에서 누군가 방 입장했을 때 유저의 입장 시간 저장
    - Room class에 최초 방 입장 시간 저장하면 될듯? or 방에 사람이 있다면 체크
    - 그래서 RoomManager에서 돌면서 각 Room마다 최초 방 입장시간 체크
    - 그래서 아직 게임 시작 안했으면 내보내기
    - 게임 시작되면 게임 시작 시점 Room별로 저장.
    - 게임 시작 시점에서 너무 오래 지났으면 내보내기

- 게임 진행 중에 한 유저의 턴이 시작된 시점부터 돌을 두지 않은 시점이 얼마나 차이나는지, 얼마나 지났는지
    - 클라에서 reqPutstone 할 때 서버에서 로직 체크하고 만약에 둘 수 있는 곳이면 유저가 돌 둔 시점에 다음 유저의 턴 시작 시점이라고 가정하고 저장(RoomUser)
    - 게임 중일 때, 현 시간에서 유저 턴 시작된 시간 뻈을 때  n분 지났으면 자동으로 턴 전환 notify
    - 턴 전환이 3번 이상 반복되면 그냥 게임 끝내고 강제로 상대방 이기고 본인 지게 만들기 notify

→Therefore, 2개의 타이머가 필요.

→혹은 일정 주기를 그냥 한번에 1개의 타이머로 검사(방 생성은 60분 지났으면 체크, 유저 게임은 1분으로 체크. 한도를 다르게 두는것

- 이 시간 체크 요청을 이너 패킷으로 전송하는데, 이 이너패킷 전송하는 주기를 짧게 두었다는 말이죠?!

## 필요한 패킷

### 방 관련

- 클라핑: OmokBoard에서 마우스 클릭 핸들러 처리할 때 시간정보..? 이건 나중에 정할 필요 있음

- Notify_Inner_Room_Check: 이게 이너패킷, 하트비트

- Notify_Skip_Turn: 턴 강제 스킵 노티파이

- Notify_Force_End_Game: 게임 강제종료 노티파이


### 유저 관련

- 클라 핑: Req_User_Auth_Info: n초마다 유저 인증정보 전송

- 서버 퐁: Res_User_Auth_info: Req 클라핑, Update 유저정보 완료 후 결과 전송

	- 퐁 처리할 때 필요 메서드: Update_User_Info: 유저 마지막 접속 확인 시간 업데이트

- 서버 검사: NTF_Inner_User_Check: 유저 상태 검사하는 유저 이너패킷

	- 서버 검사할 때 필요 메소드: UserDisconnect: 서버측에서 클라 연결 반응 일정시간동안 없으면 연결 강제 해제.


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
