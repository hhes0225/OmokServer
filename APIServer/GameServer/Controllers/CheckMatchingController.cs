using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckMatchingController:Controller
{
    private ILogger<CheckMatchingController> _logger;
    string _matchingServerAddress;

    public CheckMatchingController(ILogger<CheckMatchingController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _matchingServerAddress = configuration["MatchingServerAddress"] + "/CheckMatching";
    }

    [HttpPost]
    public async Task<CheckMatchingResponse> Post(CheckMatchingRequest request)
    {
        CheckMatchingResponse response=new CheckMatchingResponse();

        HttpClient client = new HttpClient();
        var checkMatchingResponse = await client.PostAsJsonAsync(_matchingServerAddress, new
        {
            UserID = request.UserID,
        });

        if(checkMatchingResponse.IsSuccessStatusCode) 
        {
            var responseContent = await checkMatchingResponse.Content.ReadAsStringAsync();
            responseContent = responseContent.Replace("result", "Result");

            var responseObject = JsonSerializer.Deserialize<CheckMatchingResponse>(responseContent);

            if (responseObject.Result != ErrorCode.None)
            {
                response.Result = responseObject.Result;
                return response;
            }
            else
            {
                
            }
        }
        else
        {
            response.Result = ErrorCode.MatchingFailError;
        }

        return response;
    }
}

public class CheckMatchingRequest
{
    public string UserID { get; set; } = "";
}
public class CheckMatchingResponseFromMatching
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}

public class CheckMatchingResponse
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string SocketServerAddress { get; set; } = "";
    public string SocketServerPort { get; set; } = "";
    public string RoomNumber { get; set; }="";
}
