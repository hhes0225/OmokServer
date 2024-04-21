using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ZLogger;
using APIServer.Repository;
using APIServer.Services;

namespace APIServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly IAccountDB _accountDB;
    private readonly IMemoryDB _memoryDB;
    private readonly ILogger<LoginController> _logger;
    

    public LoginController(ILogger<LoginController> logger, IAccountDB accountDB, IMemoryDB memoryDB)
    {
        _logger = logger;
        _accountDB = accountDB;
        _memoryDB = memoryDB;
    }

    [HttpPost]
    public async Task<LoginResponse> Post(LoginRequest request)
    {
        LoginResponse response=new LoginResponse();

        Tuple<ErrorCode, string> sqlDBResult = await _accountDB.VerifyUser(request.Email, request.Password);
        if(sqlDBResult.Item1 != ErrorCode.None)
        {
            response.Result = sqlDBResult.Item1;
            return response;
        }

        //인증토큰 생성
        string token = Security.GenerateAuthToken(request.Email);
        response.AuthToken= token;

        //Redis에 이메일과 인증토큰 저장
        ErrorCode errorCode = await _memoryDB.RegisterUserAsync(request.Email, token);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        //인증토큰 반환
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
