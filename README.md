# OmokServer
Developed by: 한희선

개발 기간: 2024.04.15~

프로젝트 개요: 온라인 오목 게임을 위한 게임 서버 개발

## API Server
Game Server와 계정을 관리하는 Hive Server를 구현,
이 두 서버를 통해 로그인 기능을 구현한다.
 
### 사용 기술
- DB: Mysql
- Cache DB: Redis(로그인 인증 토큰을 임시 저장)
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

  
