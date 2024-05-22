namespace GameServer.Repository;

public interface IMatchingList:IDisposable
{
    public Task<MatchingResult> MatchingUserAsync();
}
