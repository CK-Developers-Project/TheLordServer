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
        UserResistration,   // 유저 닉네임 및 종족 선택
    }

    public enum EventCode : byte
    {

    }
}
