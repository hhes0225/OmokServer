# Matching Request
<br>

```mermaid
sequenceDiagram
  autonumber
  actor A as client
  participant B as GameServer
  participant C as GameUserRedis
  participant D as MatchingServer
  participant E as MatchingRedis
  participant F as SocketServer

  A->>B: 인증정보(Id, AuthToken) 전송
  B->>C: 인증정보 검증 요청
  C->>C: 인증정보 검증
  C->>B: 인증 결과 전송

  alt 검증 성공
    B->>D: 매칭요청(ID) 전달
    D->>D: Matching Concurrent Queue에 ID 등록
    D->>B: 결과(ErrorCode) response
    B->>A: 결과 그대로 전달
  end
```

# Matching Check
<br>

```mermaid
sequenceDiagram
  autonumber
  actor A as client
  participant B as GameServer
  participant C as GameUserRedis
  participant D as MatchingServer
  participant E as MatchingRedis
  participant F as SocketServer

  A->>B: 일정 시간마다 주기적으로 인증정보(Id, AuthToken) 전송
  B->>C: 인증정보 검증 요청
  C->>C: 인증정보 검증
  C->>B: 인증 결과 전송

  alt 검증 성공
    B->>D: 매칭 체크 요청(ID) 전달
    D->>D: 매칭 성공 Dictionary에 있는지 확인
    D->>B: 있다면 pop해서 결과 전달(IP, port, RoomNum)
    B->>A: 결과 그대로 전달
    A->>A: 결과 바탕으로 소켓 서버 접속
  end
```

# Matching - Matching & Socket Server side 
<br>

```mermaid
sequenceDiagram
  autonumber
  actor A as client
  participant B as GameServer
  participant C as GameUserRedis
  participant D as MatchingServer
  participant E as MatchingRedis
  participant F as SocketServer

  A->>B: 일정 시간마다 주기적으로 인증정보(Id, AuthToken) 전송
  B->>C: 인증정보 검증 요청
  C->>C: 인증정보 검증
  C->>B: 인증 결과 전송

  alt 검증 성공
    B->>D: 매칭 체크 요청(ID) 전달
    D->>D: Matching Concurrent Queue에 ID 등록
    D->>B: 결과(ErrorCode) response
    B->>A: 결과 그대로 전달
  end
```

