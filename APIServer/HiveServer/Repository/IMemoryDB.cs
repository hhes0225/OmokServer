using System;
using System.Threading.Tasks;

namespace HiveServer.Repository;

public interface IMemoryDB : IDisposable
{
    public Task<ErrorCode> RegisterUserAsync(string email, string authToken);

    public Task<ErrorCode> CheckUserAuthAsync(string email, string authToken);

    public Task<Tuple<bool, RedisDBAuthUserData>> GetUserAsync(string email);
}