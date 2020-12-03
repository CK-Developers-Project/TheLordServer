namespace TheLordServer.Structure
{
    public enum ReturnCode
    {
        Success = 0,
        Failed = -1
    }

    public enum OperationCode : byte
    {
        // LoginHandler
        Login,              // 로그인
        UserResistration,   // 유저 닉네임 및 종족 선택

        // UserAssetHandler
        RequestResource,    // 리소스 갱신 요청

    }

    public enum EventCode : byte
    {

    }
}
