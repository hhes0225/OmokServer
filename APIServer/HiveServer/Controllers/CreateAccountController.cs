using APIServer.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ZLogger;


namespace APIServer.Controllers;

[ApiController]
[Route("[controller]")]

public class CreateAccountController : ControllerBase
{
    private readonly IAccountDB _accountDB;
    private readonly ILogger<CreateAccountController> _logger;

    public CreateAccountController(ILogger<CreateAccountController> logger, IAccountDB accountDB)
    {
        _logger = logger;
        _accountDB = accountDB; 
    }

    [HttpPost]
    public async Task<CreateAccountResponse> Post(CreateAccountRequest request)
    {
        CreateAccountResponse response= new CreateAccountResponse();
        
        //DB에 이메일 정보 조회해서 계정 존재 여부 확인
        ErrorCode errorCode = await _accountDB.FindAccountExistAsync(request.Email);

        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        //계정 생성
        errorCode = await _accountDB.CreateAccountAsync(request.Email, request.Password);

        //계정 생성 실패
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }


        return response;
    }
}

public class CreateAccountRequest
{
    [Required]
    [MinLength(1, ErrorMessage ="EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage ="EMAIL IS TOO LONG")]
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage ="EMAIL IS NOT VALID")]
    public string Email { get; set; }
    


    //Password
    [Required]
    [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage ="PASSWORD IS TOO LONG")]
    [DataType(DataType.Password)] //입력 필드가 사용자의 비밀번호를 나타낸다는 것을 지정
    public string Password { get; set; }

}

public class CreateAccountResponse
{
    public ErrorCode Result { get; set; } = ErrorCode.None;//Default is None
}
