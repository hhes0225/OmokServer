using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace APIServer.Services;

public class Security
{
    //생성되는 솔트 문자열은 무조건 영어 소문자와 숫자로 구성됨.
    private const String AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    //회원가입: Hash 암호화를 위한 Salt 문자열 생성
    public static String SaltString()
    {
        var bytes = new byte[64];//1byte*길이 64인 바이트 생성
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(bytes);
        }
        return new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());
    }

    //회원가입 & 로그인: Salt 문자열로 비밀번호 Hash 암호화
    public static string HashPassword(string saltvalue, string password)
    {
        var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(saltvalue + password));
        var stringBuilder=new StringBuilder();

        foreach(var x in hash)
        {
            stringBuilder.AppendFormat("{0:x2}", x);
        }

        return stringBuilder.ToString();
    }

    //로그인: Redis(Memory)에 저장할 토큰 생성 - JWS
    public static string GenerateAuthToken(string email)
    {
        //JWS Payload 설정
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            claims:claims,
            expires: DateTime.UtcNow.AddHours(6),
            notBefore:DateTime.UtcNow
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }

}
