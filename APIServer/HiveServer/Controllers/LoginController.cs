using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ZLogger;
using HiveServer.Repository;
using HiveServer.Services;

namespace HiveServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly IAccountDB _accountDB;
    private readonly ILogger<LoginController> _logger;
    

    public LoginController(ILogger<LoginController> logger, IAccountDB accountDB)
    {
        _logger = logger;
        _accountDB = accountDB;
    }

    [HttpPost]
    public async Task<LoginResponse> Post(LoginRequest request)
    {
        LoginResponse response=new LoginResponse();
        Tuple<ErrorCode, string> result = await _accountDB.VerifyUser(request.Email, request.Password);

        if(result.Item1 != ErrorCode.None)
        {
            response.Result = result.Item1;
            return response;
        }

        //인증토큰 생성
        string token = Security.GenerateAuthToken(request.Email);

        Console.WriteLine(token);

        //Redis에 이메일과 인증토큰 저장


        return response;
    }

}



public class LoginRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
    public string Email { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
    [StringLength(30, ErrorMessage = "PASSWORD IS TOO LONG")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}

public class LoginResponse
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string AuthToken { get; set; } = "";
}
