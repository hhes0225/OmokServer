using GameServer.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchingController : Controller
{
    private ILogger<MatchingController> _logger;
    private readonly IMatchingList _matchingList;
    string _matchingServerAddress;

    public MatchingController(ILogger<MatchingController> logger, IMatchingList matchingList,IConfiguration configuration)
    {
        _logger = logger;
        _matchingList = matchingList;
        _matchingServerAddress = configuration["MatchingServerAddress"] + "/RequestMatching";
    }

    [HttpPost]
    public async Task<MatchingResponse1> Post(MatchingReqeust1 request)
    {
        MatchingResponse1 response = new MatchingResponse1();

        var result = await _matchingList.MatchingUserAsync();

        if (result.Result == ErrorCode.None)
        {
            response.RoomNumber = result.RoomNumber;
        }
        else
        {
            response.Result = result.Result;
        }

        return response;
    }
}

public class MatchingReqeust1
{
    public string UserID { get; set; } = "";
}

public class MatchingResponse1
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string RoomNumber { get; set; } = "";
}
