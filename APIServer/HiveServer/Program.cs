using HiveServer.Services;
using HiveServer.Repository;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

//DI ���� �۾�
//mysql-Hive DB�� Account table
builder.Services.AddScoped<IAccountDB, AccountDB>();
//Redis
builder.Services.AddSingleton<ICacheDB, CacheDB>();


// Add services to the container.
builder.Services.AddControllers();

WebApplication app = builder.Build();


app.UseRouting();

app.UseAuthorization();

app.Run();
