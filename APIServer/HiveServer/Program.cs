using HiveServer.Services;
using HiveServer.Repository;
using ZLogger;
using System.Configuration;

//ASP.NET Core 웹 애플리케이션을 구성하기 위한 새로운 빌더 객체를 생성
WebApplicationBuilder builder =  WebApplication.CreateBuilder(args);

//Repository 등록
//DI 패턴
//ex) "IAccountDB" 인터페이스와 "AccountDB" 클래스 간의 의존성을 등록
builder.Services.AddScoped<IAccountDB, AccountDB>();


//웹 API 관련 모듈 사용 선언: IServiceCollecion 컨트롤러 등록
builder.Services.AddControllers();

//애플리케이션의 설정 정보를 가져옴.
//설정 ex) 애플리케이션의 포트 번호 or 데이터베이스 연결 정보 등(appsetting.json?)
IConfiguration configuration = builder.Configuration;
//builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));

//WebApplicationBuilder 객체를 사용하여 ASP.NET Core 웹 애플리케이션을 빌드
//애플리케이션을 실행 가능한 형태로 만들고, 그 결과를 변수에 저장
var app = builder.Build();

//컨트롤러의 액션 메서드를 호출하고 해당 액션 메서드에서 반환한 데이터를 클라이언트에게 반환
//클라이언트는 주로 JSON 형식으로 데이터를 받게 됨
app.MapDefaultControllerRoute();

//서버 주소로 애플리케이션을 실행하고 사용자의 요청에 대한 응답을 처리
app.Run(configuration["ServerAddress"]);