using System;
using System.Threading.Tasks;

namespace APIServer.Repository;

public interface IAccountDB : IDisposable
{
    public Task<ErrorCode> FindAccountExistAsync(string email);

    //계정 생성 메소드
    //주어진 이메일 주소와 비밀번호를 받아서 데이터베이스에 새로운 계정을 추가하고,
    //이 과정에서 발생하는 오류를 처리
    //arguments: 이메일, 비밀번호
    //return: Task<TResult> = 일반적으로 비동기적으로 실행되는 단일 작업
    //        ErrorCode를 반환(ErrorCode.cs 참고, 정상 동작/비정상 동작 시 결과 코드 반환)
    public Task<ErrorCode> CreateAccountAsync(string email, string pw);

    //이메일 주소와 비밀번호를 검증하여 사용자를 인증하고,
    //인증에 성공한 경우에는 사용자의 식별자(ID, email 등)를 반환
    //arguments: 이메일, 비밀번호
    //return: 메소드 정상/비정상 동작+원인 파악하기 위한 ErrorCode & 유저 이메일
    public Task<Tuple<ErrorCode, string>> VerifyUser(String email, string pw);
}
