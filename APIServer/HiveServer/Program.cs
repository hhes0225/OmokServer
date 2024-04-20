using HiveServer.Services;
using HiveServer.Repository;
using ZLogger;
using System.Configuration;

WebApplicationBuilder builder =  WebApplication.CreateBuilder(args);

//Repository 등록
builder.Services.AddScoped<IAccountDB, AccountDB>();
builder.Services.AddSingleton<IMemoryDB, MemoryDB>();

builder.Services.AddControllers();

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));

var app = builder.Build();

app.MapDefaultControllerRoute();

app.Run(configuration["ServerAddress"]);
