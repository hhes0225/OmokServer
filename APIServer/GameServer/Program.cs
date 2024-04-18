var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

//DI ���� �۾�
//mysql-Omok DB�� User table
builder.Services.AddScoped<IUserDB, UserDB>();
//Redis
builder.Services.AddSingleton<ICacheDB, CacheDB>();


// Add services to the container.
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseRouting();

app.UseAuthorization();

app.Run();
