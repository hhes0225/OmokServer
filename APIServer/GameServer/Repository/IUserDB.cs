namespace GameServer.Repository;

public interface IUserDB : IDisposable
{
    public Task<dbUserInfo> FindUserDataAsync(string email);
    public Task<ErrorCode> InsertUserAsync(string email);
    public Task<int> UpdateRecentLogin(int uid);
}
