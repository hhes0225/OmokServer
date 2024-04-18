using HiveServer.Services;
using HiveServer.Repository;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

//DI 관련 작업
//mysql-Hive DB의 Account table
builder.Services.AddScoped<IAccountDB, AccountDB>();
//Redis
builder.Services.AddSingleton<ICacheDB, CacheDB>();


// Add services to the container.
builder.Services.AddControllers();

WebApplication app = builder.Build();


app.UseRouting();

app.UseAuthorization();

app.Run();
