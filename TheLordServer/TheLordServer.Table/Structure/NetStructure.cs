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

        // RankingHandler
        RequestRaidRanking, // 레이드 랭킹 갱신 요청
        ResultRaidRanking,  // 레이드 랭킹 결과

        // ContentHandler
        EnterContent,       // 콘텐츠 입장
        EnterRaid,          // 레이드 입장

        // ChatHandler
        Chat,
    }

    public enum EventCode : byte
    {
        // UserEvent
        UpdateChat,         // 채팅 갱신

        // UserAssetEvent
        UpdateResource,     // 리소스 갱신

        // BuidlingEvent
        UpdateBuilding,    // 건물 하나 갱신

        // RankingEvent
        UpdateRaidRanking,  // 레이드 랭킹 갱신

        // BossEvent
        UpdateRaidBoss,     // 레이드 보스 갱신
        ExitRaidBoss,       // 레이드 보스씬 퇴장 (누군가 보스를 잡았음)
    }
}