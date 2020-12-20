namespace TheLordServer.Table.Structure
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
        LobbyEnter,         // 로비에 입장

        // UserAssetHandler
        RequestResource,    // 리소스 갱신 요청

        // BuildingHandler
        BuildingClick,      // 건물 클릭 처리 요청
        BuildingConfirm,    // 건물 확인 요청

    }

    public enum EventCode : byte
    {
        // UserEvent
           

        // UserAssetEvent
        UpdateResource,     // 리소스 갱신
    }
}