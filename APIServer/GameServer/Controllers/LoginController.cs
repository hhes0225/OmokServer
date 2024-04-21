using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using GameServer.Repository;
using Microsoft.AspNetCore.Identity.Data;
using System.Text.Json;
//using GameServer.Services;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : Controller
{
    private readonly IUserDB _userDB;
    private readonly IMemoryDB _memoryDB;
    private readonly ILogger<LoginController> _logger;
    string _hiveServerAddress;

    public LoginController(ILogger<LoginController> logger, IUserDB userDB, IMemoryDB memoryDB, IConfiguration configuration)
    {
        _logger = logger;
        _userDB = userDB;
        _memoryDB = memoryDB;
        _hiveServerAddress = configuration["HiveServerAddress"] + "/VerifyToGameServer";
        Console.WriteLine(_hiveServerAddress);
    }

    [HttpPost]
    public async Task<LoginResponse> Post(LoginRequest request)
    {
        LoginResponse response = new LoginResponse();

        //client request�� �����Ͽ� HiveServer�� ����
        HttpClient client = new HttpClient();
        var hiveResponse = await client.PostAsJsonAsync(_hiveServerAddress, new
        {
            Email = request.Email,
            HiveToken = request.AuthToken
        });

        if (hiveResponse.IsSuccessStatusCode)
        {
            //json �������� ���� ���� �о����
            var responseContent = await hiveResponse.Content.ReadAsStringAsync();
            responseContent = responseContent.Replace("result", "Result");

            //���� ���� json���� deserialize�Ͽ� ��ü�� ��ȯ
            var responseObject = JsonSerializer.Deserialize<LoginResponse>(responseContent);

            // ��� enum �� ����
            var result = responseObject.Result;

            if (responseObject.Result != ErrorCode.None)
            {
                response.Result = responseObject.Result;
                return response;
            }
        }
        else
        {
            response.Result = ErrorCode.LoginFailHiveConnectionException;
            return response;
        }

        //Mysql: �̸��� ������ ���� ������ �ε�
        var userData = _userDB.FindUserDataAsync(request.Email);


        //���� ������� ���� ����
        if (userData.Result == null)
        {
            ErrorCode insertData = await _userDB.InsertUserAsync(request.Email);

            if (insertData != ErrorCode.None)
            {
                response.Result=ErrorCode.UserDBInsertException;
                return response;
            }

            //���� ������ �ε�
            userData = _userDB.FindUserDataAsync(request.Email);
        }

        //Redis�� �̸��ϰ� ������ū ����(���� �ΰ��ӿ����� �� ������ ����Ͽ� ����)
        ErrorCode errorCode = await _memoryDB.RegisterUserAsync(request.Email, request.AuthToken);
        if(errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }


        //���� �α��� ��� ����
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
    [MinLength(1, ErrorMessage = "AUTHTOKEN CANNOT BE EMPTY")]
    public string AuthToken { get; set; }
}

public class LoginResponse
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}
