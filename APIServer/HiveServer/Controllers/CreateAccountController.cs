using HiveServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ZLogger;


namespace HiveServer.Controllers;

//컨트롤러가 API 엔드포인트를 처리하는 데 사용
[ApiController]
//해당 컨트롤러의 경로를 정의하여 요청을 컨트롤러와 연결
//컨트롤러의 이름이 CreateAccountController라면, [Route("[controller]")]는 /createaccount로 요청을 라우팅
[Route("[controller]")]


//MVC 애플리케이션에서는 Controller 상속
//웹 api에서는 ControllerBase 상속(View 같은 불필요한 기능을 제거하고 API 컨트롤러를 더 가볍게 유지)
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
    //컨트롤러 메서드(액션 메서드)가 HTTP POST 요청을 처리
    //클라이언트가 서버에게 데이터를 제출하고자 할 때(주로 폼 데이터나 JSON 데이터) 사용

    //계정 생성이므로 이메일, 비밀번호가 들어오면 DB에 조회
    //있으면 Du
    public async Task<CreateAccountResponse> Post(CreateAccountRequest request)
    {
        CreateAccountResponse response= new CreateAccountResponse(); // Response 메시지 객체 생성

        ////일단 정보 조회해서 있는지 확인
        ////ErrorCode errorCode = await _accountDB.CreateAccountAsync(request.Email, request.Password);
        //ErrorCode = ErrorCode.None;

        //if (errorCode != ErrorCode.None)
        //{
        //    response.Result=errorCode;
        //    return response;
        //}
       

        return response;
    }
}

public class CreateAccountRequest
{
    //EMAIL
    [Required]
    //이 속성이 반드시 있어야 함을 의미(데이터 유효성 검사 규칙을 정의)
    [MinLength(1, ErrorMessage ="EMAIL CANNOT BE EMPTY")]
    //최소 길이를 1로 설정, 길이가 1보다 작다면 에러메시지 출력
    [StringLength(50, ErrorMessage ="EMAIL IS TOO LONG")]
    //최대 길이를 50으로 설정, 길이가 50보다 크다면 에러메시지 출력
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage ="EMAIL IS NOT VALID")]
    //^: 문자열의 시작, $: 문자열의 끝
    //[a-zA-Z0-9_\\.-]+: 영어, 숫자, 특수기호(_ . -)중 하나가 연속으로 사용
    //@: 이 기호가 나와야 함.
    //([a-zA-Z0-9-]+\\.)+:소문자 대문자 숫자, 대시 -중 하나가 나와야 하며 그 뒤에 점이 나와야 함
    //                  괄호 쓴 이유: mail.gmail.(ok) / mail..(not ok X): .온점이 연속적으로 나타나는 것을 방지
    // 온점(.) 앞에는  무조건 문자열이 나와야 함.
    //[a-zA-Z]{2,6}: 영어 대소문자가 2-6자여야 할 것(com, kr 등..)
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
