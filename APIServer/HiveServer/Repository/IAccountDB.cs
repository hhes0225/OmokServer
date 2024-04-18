using System;
using System.Threading.Tasks;

namespace HiveServer.Services;

public interface IAccountDB : IDisposable
{
    //계정 생성 메소드
    //arguments: 이메일, 비밀번호
    //return: Task<TResult> = 일반적으로 비동기적으로 실행되는 단일 작업
    //        ErrorCode를 반환(ErrorCode.cs 참고, 정상 동작/비정상 동작 시 결과 코드 반환)
    public Task<ErrorCode> CreateAccountAsync(string email, string pw);

    //계정 인증여부 반환 메소드
    //arguments: 이메일, 비밀번호
    //return: 메소드 정상/비정상 동작+원인 파악하기 위한 ErrorCode & 유저 이메일
    public Task<Tuple<ErrorCode, Int64>> VerifyUser(String email, string pw);
}
