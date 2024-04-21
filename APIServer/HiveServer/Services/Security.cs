using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace APIServer.Services;

public class Security
{
    //�����Ǵ� ��Ʈ ���ڿ��� ������ ���� �ҹ��ڿ� ���ڷ� ������.
    private const String AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    //ȸ������: Hash ��ȣȭ�� ���� Salt ���ڿ� ����
    public static String SaltString()
    {
        var bytes = new byte[64];//1byte*���� 64�� ����Ʈ ����
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(bytes); //64����Ʈ�� �������� ä���ֱ�(������)
        }
        //RandomNumberGenerator.Create().GetBytes(bytes)�� �ص� �Ǵµ�,
        //using�� ���� ��� �� �ٷ� Dispose�ؼ� �޸� ������ ����������.

        //char = 1byte�̹Ƿ� 1byte���� �����Ǵ� char�� ��ȯ
        //�̶�, AllowableCharacters �迭�� �ִ� ���ڷθ� ��ȯ���ش�.
        return new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());
    }

    //ȸ������ & �α���: Salt ���ڿ��� ��й�ȣ Hash ��ȣȭ
    //SHA-256 �ؽ��� 16���� ���ڿ� ǥ���� �׻� 64�ڸ�
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

    //�α���: Redis(Memory)�� ������ ��ū ���� - JWS
    public static string GenerateAuthToken(string email)
    {
        //JWS Payload ����
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