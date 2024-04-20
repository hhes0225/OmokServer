using System;
using System.Threading.Tasks;

namespace HiveServer.Repository;

public interface IAccountDB : IDisposable
{
    public Task<ErrorCode> FindAccountExistAsync(string email);

    public Task<ErrorCode> CreateAccountAsync(string email, string pw);

    public Task<Tuple<ErrorCode, string>> VerifyUser(String email, string pw);
}
