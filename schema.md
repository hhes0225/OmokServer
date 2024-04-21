# Hive database

### Account table

```
Use Hive;

# SHA-256 해시의 16진수 문자열 표현은 항상 64자리
CREATE TABLE Account(
    uid BIGINT NOT NULL AUTO_INCREMENT COMMENT '유저 고유번호',
    email VARCHAR(50) NOT NULL COMMENT '이메일',
    pw VARCHAR(100) NOT NULL COMMENT '비밀번호',
    salt_value VARCHAR(100) NOT NULL COMMENT '암호화 값',
    create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '생성 일시',
    recent_login_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '최근 로그인 일시',
    PRIMARY KEY(uid),
    UNIQUE(email)
)COMMENT '계정 정보 테이블';
```

# Game Server database

### User table

```
Use GameDB;

CREATE TABLE User(
	uid BIGINT NOT NULL AUTO_INCREMENT COMMENT '유저 고유번호',
	email VARCHAR(50) NOT NULL COMMENT '이메일',
	nickname VARCHAR(30) NOT NULL COMMENT '닉네임',
	create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '생성 일시',
	recent_login_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '최근 로그인 일시',
	win_count INT NOT NULL DEFAULT 0 COMMENT '이긴 횟수',
	draw_count INT NOT NULL DEFAULT 0 COMMENT '비긴 횟수',
	lose_count INT NOT NULL DEFAULT 0 COMMENT '진 횟수',
	PRIMARY KEY(uid),
	UNIQUE(email),
	UNIQUE(nickname)
)COMMENT '게임 유저 정보 테이블';
```
