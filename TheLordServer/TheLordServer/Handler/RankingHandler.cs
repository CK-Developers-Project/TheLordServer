using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Event;

    public class RankingHandler : Singleton<RankingHandler>, IBaseHandler
    {
        public void AddListener ( )
        {
            HandlerMedia.AddListener ( OperationCode.RequestRaidRanking, OnRequestRaidRankingReceived );
            HandlerMedia.AddListener ( OperationCode.ResultRaidRanking, OnResultRaidRankingReceived );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveAllListener ( OperationCode.RequestRaidRanking );
            HandlerMedia.RemoveAllListener ( OperationCode.ResultRaidRanking );
        }


        RaidRankingData CreateRaidRankingData (ClientPeer peer)
        {
            RaidRankingData data = new RaidRankingData ( peer.Id );
            data.Nickname = peer.userAgent.UserData.Info.Nickname;
            data.Index = peer.userAgent.UserAssetData.Index;
            data.Tier = peer.userAgent.UserAssetData.Tier;
            return data;
        }


        void OnRequestRaidRankingReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            var workRankingList = MongoHelper.RaidRankingCollection.GetAllSorting ( ).GetAwaiter ( );
            workRankingList.OnCompleted ( ( ) =>
              {
                  var rankingDataList = workRankingList.GetResult ( );
                  if(rankingDataList == null || rankingDataList.Count == 0)
                  {
                      RaidRankingData data = CreateRaidRankingData ( peer );
                      var workRankingAdd = MongoHelper.RaidRankingCollection.Add ( data ).GetAwaiter ( );
                      workRankingAdd.OnCompleted ( ( ) =>
                        {
                            RankingEvent.OnUpdateRaidRanking ( peer, data, null, new List<RaidRankingData> { data } );
                            BossEvent.OnUpdateRaidBoss ( peer, 16 );
                        } );
                      return;
                  }

                  // TODO : 보스 디졌는지 안디졌는지 확인

                  var myRankingData = rankingDataList.Find ( x => x.Key.Equals ( peer.Id ) );
                  var lastHitData = rankingDataList.Find ( x => x.LastHit == true );
                  if(myRankingData == null)
                  {
                      myRankingData = CreateRaidRankingData ( peer );
                      var workRankingAdd = MongoHelper.RaidRankingCollection.Add ( myRankingData ).GetAwaiter ( );
                      workRankingAdd.OnCompleted ( ( ) =>
                      {
                            rankingDataList.Add ( myRankingData );
                            RankingEvent.OnUpdateRaidRanking ( peer, myRankingData, lastHitData, rankingDataList );
                            BossEvent.OnUpdateRaidBoss ( peer, 16 );
                      } );
                  }
                  else
                  {
                      RankingEvent.OnUpdateRaidRanking ( peer, myRankingData, lastHitData, rankingDataList );
                      BossEvent.OnUpdateRaidBoss ( peer, 16 );
                  }
              } );
            
           
        }
    
        void OnResultRaidRankingReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            if( TheLordServer.Instance.bossDataList.Count == 0 )
            {
                return;
            }
            var data = BinSerializer.ConvertData<ProtoData.RaidRankingScoreData> ( operationRequest.Parameters );

            TheLordServer.Log.InfoFormat ( "{0}의 플레이어가 {1}의 피해를 입혔습니다.", peer.userAgent.UserData.Info.Nickname, data.score );

            var workRankingScore = MongoHelper.RaidRankingCollection.Get ( peer.Id ).GetAwaiter ( );
            workRankingScore.OnCompleted ( ( ) =>
              {
                  var rankingScore = workRankingScore.GetResult ( );

                  TheLordServer.Instance.bossDataList[0].HP -= data.score;
                  if ( TheLordServer.Instance.bossDataList[0].HP <= 0F )
                  {
                      rankingScore.LastHit = true;
                      BossEvent.OnExitRaidBoss ( );
                  }
                  else
                  {
                      foreach(var building in peer.userAgent.BuildingDataList )
                      {
                          int amount = building.CharactertData.Amount;
                          int subract = (int)Math.Ceiling ( amount * 0.66F );
                          building.CharactertData.Amount = Math.Max ( 0, amount - subract );
                      }
                  }

                  rankingScore.Score += data.score;
                  var workUpdateRankingScore = MongoHelper.RaidRankingCollection.Update ( rankingScore ).GetAwaiter();
                  workUpdateRankingScore.OnCompleted ( ( ) =>
                    {
                        
                    } );
              } );
        }
    }
}
