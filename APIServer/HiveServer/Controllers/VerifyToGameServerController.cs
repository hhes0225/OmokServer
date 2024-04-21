using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using APIServer.Repository;
using APIServer.Services;
using ZLogger;

namespace APIServer.Controllers;

[ApiController]
[Route("[controller]")]

public class VerifyToGameServerController : ControllerBase
{
    readonly string _token;
    readonly ILogger<VerifyToGameServerController> _logger;
    readonly IMemoryDB _memoryDB;

    public VerifyToGameServerController(ILogger<VerifyToGameServerController>logger, IMemoryDB memoryDB, IConfiguration config)
    {
        _logger = logger;
        _memoryDB = memoryDB;
    }

    [HttpPost]
    public async Task<VerifyTokenResponse> Verify([FromBody] VerifyTokenRequestBody request)
    {
        Console.WriteLine("Verify...\n");
        VerifyTokenResponse response = new VerifyTokenResponse();
        ErrorCode errorCode = await _memoryDB.CheckUserAuthAsync(request.Email, request.HiveToken);

        if(errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        
        return response;
    }
}

public class VerifyTokenRequestBody
{
    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string HiveToken { get; set; } = "";
}

public class VerifyTokenResponse
{
    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}