# OmokServer
Developed by: 한희선

개발 기간: 2024.04.15~

프로젝트 개요: 온라인 오목 게임을 위한 게임 서버 개발

## API Server
로그인을 요청하는 Game Server와 계정을 관리하는 Hive Server를 구현,
이 두 서버를 통해 로그인 기능을 구현한다.
 
### 사용 기술
- DB: Mysql
- Memory DB: Redis(로그인 인증 토큰을 임시 저장)
- Server: ASP.NET core 웹 어플리케이션

### 작업 내역
#### 초기 작업
|구현 여부|구현 사항|
|------|------|
|✅|DB 스키마 정의|
|✅|시퀀스 다이어그램 작성|
|✅|SQL DB 생성|
|✅|Redis DB 도커로 띄우기|

#### Hive Server
|구현 여부|구현 사항|
|------|------|
|✅|DB와 프로젝트 연결|
|✅|Redis와 프로젝트 연결|
|✅|Repository-mysql 연동, 정보 조회, 데이터 삽입|
|✅|Repository-redis 연동, 데이터 삽입|
|⛔|DAO, DTO로 파일 분리|
|✅|Controller-계정 존재 여부 확인|
|✅|Controller-계정 생성 시 비밀번호 암호화|
|✅|Controller-계정 생성 동작 처리|
|✅|Controller-로그인|
|✅|Controller-인증토큰 생성|
|✅|Controller-인증토큰 Redis에 저장|


#### Game Server
|구현 여부|구현 사항|
|------|------|
|✅|DB와 프로젝트 연결|
|✅|Redis와 프로젝트 연결|
|✅|Repository 구현(인터페이스, DB에서 데이터베이스 관련 작업)|
|✅|Controller: client의 요청 Hive로 전달|
|✅|Controller: HiveServer에서 조회, 결과값 반환|
|✅|Controller: User 조회, 없다면 생성|
|✅|Controller: email, 인증토큰 Redis에 저장|
|⛔|DAO, DTO로 파일 분리|

## Socket Server
인게임에서 유저 관리 / 채팅 기능 / 방 입장 기능 / 오목 로직 기능을 수행하는 서버를 구현한다.

### 작업 내역
|구현 여부|구현 사항|
|------|------|
|✅|서버 패킷 관련 사항 구현|
|✅|채팅 기능|
|✅|client와 연결|
|✅|client 패킷 수정|
|✅|방 입장 및 관리 관련 기능 구현|
|✅|게임 준비&시작 로직 및 패킷 req, res 구현|
|✅|클라이언트, 서버 공통 사용 소스 라이브러리화|
|✅|오목 게임 로직 구현(client side)|
|✅|오목 게임 로직 구현(server side)|
|✅|유저 heartbeat|
|✅|방 상태 체크|
|✅|클라이언트 요구조건에 맞게 형태 변경|
|✅|실제 서버에 띄워서 테스트(GCP 사용 예정)|
|✅|client-API 서버 연동|
|✅|Mysql 연동|
|✅|Redis 연동|
|⛔|단순한 로직의 매칭 서버 구현|
