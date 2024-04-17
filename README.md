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
|구현 여부|구현 사항|
|------|------|
|✅|DB 스키마 정의|
|⛔|시퀀스 다이어그램 작성|
|⛔|DB 생성|
|⛔|DB와 프로젝트 연결|
|⛔|DAO, DTO 작성|
  
