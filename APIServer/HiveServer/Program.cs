using HiveServer.Services;
using HiveServer.Repository;
using ZLogger;
using System.Configuration;

//ASP.NET Core �� ���ø����̼��� �����ϱ� ���� ���ο� ���� ��ü�� ����
WebApplicationBuilder builder =  WebApplication.CreateBuilder(args);

//Repository ���
//DI ����
//ex) "IAccountDB" �������̽��� "AccountDB" Ŭ���� ���� �������� ���
builder.Services.AddScoped<IAccountDB, AccountDB>();


//�� API ���� ��� ��� ����: IServiceCollecion ��Ʈ�ѷ� ���
builder.Services.AddControllers();

//���ø����̼��� ���� ������ ������.
//���� ex) ���ø����̼��� ��Ʈ ��ȣ or �����ͺ��̽� ���� ���� ��(appsetting.json?)
IConfiguration configuration = builder.Configuration;
//builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));

//WebApplicationBuilder ��ü�� ����Ͽ� ASP.NET Core �� ���ø����̼��� ����
//���ø����̼��� ���� ������ ���·� �����, �� ����� ������ ����
var app = builder.Build();

//��Ʈ�ѷ��� �׼� �޼��带 ȣ���ϰ� �ش� �׼� �޼��忡�� ��ȯ�� �����͸� Ŭ���̾�Ʈ���� ��ȯ
//Ŭ���̾�Ʈ�� �ַ� JSON �������� �����͸� �ް� ��
app.MapDefaultControllerRoute();

//���� �ּҷ� ���ø����̼��� �����ϰ� ������� ��û�� ���� ������ ó��
app.Run(configuration["ServerAddress"]);