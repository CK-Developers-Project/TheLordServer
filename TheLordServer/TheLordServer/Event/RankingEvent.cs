using Photon.SocketServer;
using System.Collections.Generic;
using System;

namespace TheLordServer.Event
{
    using MongoDB.CollectionData;
    using Table.Structure;
    using Util;

    public class RankingEvent
    {
        static ProtoData.RaidRankingData.RankingData RaidRankingData2Proto (RaidRankingData raidRankingData )
        {
            var rankingData = new ProtoData.RaidRankingData.RankingData ( );
            rankingData.nickname = raidRankingData.Nickname;
            rankingData.index = raidRankingData.Index;
            rankingData.tier = raidRankingData.Tier;
            rankingData.score = raidRankingData.Score;
            return rankingData;
        }


        public static void OnUpdateRaidRanking(ClientPeer peer, RaidRankingData myRankingData, RaidRankingData lastHitRankingData, List<RaidRankingData> raidRankingDataList)
        {
            var packet = new ProtoData.RaidRankingData ( );
            if(myRankingData != null)
            {
                int ranking = raidRankingDataList.FindIndex ( x => x.Equals ( myRankingData ) );
                packet.myRankingData = RaidRankingData2Proto ( myRankingData );
                packet.myRankingData.ranking = ranking + 1;


            }
            if(lastHitRankingData != null)
            {
                int ranking = raidRankingDataList.FindIndex ( x => x.Equals ( lastHitRankingData ) );
                packet.lastHitRankingData = RaidRankingData2Proto ( lastHitRankingData );
                packet.lastHitRankingData.ranking = ranking + 1;
            }

            int cnt = 0;
            foreach ( var rankingData in raidRankingDataList )
            {
                if(++cnt > 10)
                {
                    break;
                }
                var data = RaidRankingData2Proto ( rankingData );
                data.ranking = cnt;
                packet.rankingDataList.Add ( data );
            }

            var current = GameUtility.Now ( ) - TheLordServer.Instance.bossDataList[0].CreateTime;
            var total = GameThread.ThreadRaidBoss.bossRemainTime - current;
            packet.tick = total.Ticks;

            EventData eventData = new EventData ( (byte)EventCode.UpdateRaidRanking );
            eventData.Parameters = BinSerializer.ConvertPacket ( packet );
            peer.SendEvent ( eventData, new SendParameters ( ) );
        }
    }
}
