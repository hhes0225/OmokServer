using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchingRequestController:Controller
{
    private ILogger<MatchingRequestController> _logger;
    string _matchingServerAddress;

    public MatchingRequestController(ILogger<MatchingRequestController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _matchingServerAddress = configuration["MatchingServerAddress"] + "/RequestMatching";
    }

    [HttpPost]
    public async Task<MatchingResponse> Post(MatchingReqeust request)
    {
        MatchingResponse response = new MatchingResponse();

        //request message 생성하여 Matching Server에 전달
        HttpClient client = new HttpClient();
        var matchingResponse = await client.PostAsJsonAsync(_matchingServerAddress, new
        {
            UserID = request.UserID,
        });

        if (matchingResponse.IsSuccessStatusCode)
        {
            var responseContent = await matchingResponse.Content.ReadAsStringAsync();
            responseContent = responseContent.Replace("result", "Result");

            var responseObject = JsonSerializer.Deserialize<MatchingResponse>(responseContent);

            if (responseObject.Result != ErrorCode.None)
            {
                response.Result = responseObject.Result;
                return response;
            }
        }
        else
        {
            response.Result = ErrorCode.MatchingFailError;
        }

        return response;
    }
}

public class MatchingReqeust
{
    public string UserID { get; set; } = "";
}

public class MatchingResponse
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}
