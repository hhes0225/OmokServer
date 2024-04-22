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
    }

    [HttpPost]
    public async Task<LoginResponse> Post(LoginRequest request)
    {
        LoginResponse response = new LoginResponse();

        //client request를 생성하여 HiveServer에 전달
        HttpClient client = new HttpClient();
        var hiveResponse = await client.PostAsJsonAsync(_hiveServerAddress, new
        {
            Email = request.Email,
            HiveToken = request.AuthToken
        });

        if (hiveResponse.IsSuccessStatusCode)
        {
            //json 형식으로 응답 내용 읽어오기
            var responseContent = await hiveResponse.Content.ReadAsStringAsync();
            responseContent = responseContent.Replace("result", "Result");

            //응답 내용 json으로 deserialize하여 객체로 변환
            var responseObject = JsonSerializer.Deserialize<LoginResponse>(responseContent);

            // 결과 enum 값 추출
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

        //Mysql: 이메일 정보로 유저 데이터 로드
        var userData = _userDB.FindUserDataAsync(request.Email);


        //없는 유저라면 새로 생성
        if (userData.Result == null)
        {
            ErrorCode insertData = await _userDB.InsertUserAsync(request.Email);

            if (insertData != ErrorCode.None)
            {
                response.Result=ErrorCode.UserDBInsertException;
                return response;
            }

            //유저 데이터 로드
            userData = _userDB.FindUserDataAsync(request.Email);
        }

        //Redis에 이메일과 인증토큰 저장(추후 인게임에서는 이 정보만 사용하여 인증)
        ErrorCode errorCode = await _memoryDB.RegisterUserAsync(request.Email, request.AuthToken);
        if(errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }


        //최종 로그인 결과 리턴
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
