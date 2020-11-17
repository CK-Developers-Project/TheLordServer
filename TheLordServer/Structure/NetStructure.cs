namespace TheLordServer.Structure
{
    public enum ReturnCode
    {
        Success = 0,
        Failed = -1
    }

    public enum OperationCode : byte
    {
        Login,          // 로그인
        CreateNickname, // 첫 닉네임 입력
        CreateRace,     // 첫 종족 선택
    }

    public enum EventCode : byte
    {

    }
}
