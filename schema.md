# Hive database
### Account table
```
Use Hive;

CREATE TABLE Account(
	email VARCHAR(50) NOT NULL COMMENT '이메일',
	pw VARCHAR(30) NOT NULL COMMENT '비밀번호',
	create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '생성 일시',
	recent_login_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '최근 로그인 일시',
	PRIMARY KEY(email)
)
```

# Game Server database
### User table
```
Use Omok;

CREATE TABLE User(
	email VARCHAR(50) NOT NULL COMMENT '이메일',
	nickname VARCHAR(30) NOT NULL COMMENT '닉네임',
	create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '생성 일시',
	recent_login_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '최근 로그인 일시',
	win_count INT NOT NULL DEFAULT 0 COMMENT '이긴 횟수',
	draw_count INT NOT NULL DEFAULT 0 COMMENT '비긴 횟수',
	lose_count INT NOT NULL DEFAULT 0 COMMENT '진 횟수',
	PRIMARY KEY(email)
)
```
