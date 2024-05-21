//using GameServer.Services;
using GameServer.Repository;
using System.Configuration;
using ZLogger;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//Repository 저장
builder.Services.AddScoped<IUserDB, UserDB>();
builder.Services.AddSingleton<IMemoryDB, MemoryDB>();

builder.Services.AddControllers();

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));


var app = builder.Build();

app.MapDefaultControllerRoute();

app.Run(configuration["ServerAddress"]);
