using System.Threading;
using System;
using System.Collections.Generic;

namespace TheLordServer.GameThread
{
    using Table;
    using Table.Structure;
    using Util;
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Event;
    using System.Threading.Tasks;

    public class ThreadRaidBoss
    {
        const int RaidBossIndex = 16;
        const int UpdateMin = 1;

        Thread runnable;
        TimeSpan updateTick = new TimeSpan ( 0, UpdateMin, 0 );
        DateTime updateBoss;

        public static TimeSpan bossRemainTime = new TimeSpan ( 6, 0, 0 );

        public void Start()
        {
            runnable = new Thread ( Update );
            runnable.IsBackground = true;
            runnable.Start ( );
        }

        public void Stop()
        {
            try
            {
                runnable.Abort ( );
            }
            catch(Exception)
            {}
        }


        void Update()
        {
            try
            {
                Thread.Sleep ( 5000 );

                updateBoss = GameUtility.Now ( ) + updateTick;

                TheLordServer.Log.Info ( "ThreadRaidBoss 스레드 시작" );
                while ( true )
                {
                    Thread.Sleep ( 1000 );
                    UpdateTime ( );
                }
            }
            catch(ThreadAbortException)
            {
                //TheLordServer.Log.Error ( e.Message );
                //TheLordServer.Log.ErrorFormat ( "[UpdateTime] ", e.Message );
            }
        }

        void ResetRaid()
        {
            TheLordServer.Log.Info ( "레이드 보스 리셋 시작" );
            var bossData = TheLordServer.Instance.bossDataList[0];

            var sheet = TheLordTable.Instance.CharacterTable.CharacterInfoSheet;
            var record = BaseTable.Get ( sheet, "index", RaidBossIndex );
            bossData.HP = (int)(float)record["hp"];
            bossData.CreateTime += bossRemainTime;

            var workRankingList = MongoHelper.RaidRankingCollection.GetAllSorting ( ).GetAwaiter ( );
            workRankingList.OnCompleted ( ( ) =>
              {
                  var result = workRankingList.GetResult ( );

                  var lastHitPlayer = result.Find ( x => x.LastHit == true );
                  if ( lastHitPlayer != null )
                  {
                      lastHitPlayer.Tier = (int)TierType.God;
                  }

                  const int legend = 1;
                  const int Master = 3;
                  const int Diamond = 5;
                  const int Platinum = 10;
                  const int Gold = 20;
                  const int Silver = 40;
                  const int Bronze = 80;
                  const int Iron = 200;

                  for (int i = 0; i < result.Count; ++i )
                  {
                      if( result[i] .Score <= 0)
                      {
                      }
                      else if(i < legend )
                      {
                          result[i].Tier = (int)TierType.Lengend > result[i].Tier ? (int)TierType.Lengend : result[i].Tier;
                      }
                      else if(i < Master )
                      {
                          result[i].Tier = (int)TierType.Master > result[i].Tier ? (int)TierType.Master : result[i].Tier;
                      }
                      else if ( i < Diamond )
                      {
                          result[i].Tier = (int)TierType.Diamond > result[i].Tier ? (int)TierType.Diamond : result[i].Tier;
                      }
                      else if ( i < Platinum )
                      {
                          result[i].Tier = (int)TierType.Platinum > result[i].Tier ? (int)TierType.Platinum : result[i].Tier;
                      }
                      else if ( i < Gold )
                      {
                          result[i].Tier = (int)TierType.Gold > result[i].Tier ? (int)TierType.Gold : result[i].Tier;
                      }
                      else if ( i < Silver )
                      {
                          result[i].Tier = (int)TierType.Silver > result[i].Tier ? (int)TierType.Silver : result[i].Tier;
                      }
                      else if ( i < Bronze )
                      {
                          result[i].Tier = (int)TierType.Bronze > result[i].Tier ? (int)TierType.Bronze : result[i].Tier;
                      }
                      else if ( i < Iron )
                      {
                          result[i].Tier = (int)TierType.Iron > result[i].Tier ? (int)TierType.Iron : result[i].Tier;
                      }
                      else
                      {
                          if(result[i].Score > 0)
                          {
                              result[i].Tier = (int)TierType.Challanger > result[i].Tier ? (int)TierType.Challanger : result[i].Tier;
                          }
                      }

                      result[i].Score = 0;
                      result[i].LastHit = false;
                      var workUpdateRankingScore = MongoHelper.RaidRankingCollection.Update ( result[i] ).GetAwaiter ( );
                      workUpdateRankingScore.OnCompleted ( ( ) =>
                      {
                      } );
                  }

                  foreach(var client in TheLordServer.Instance.PeerList)
                  {
                      if( client .userAgent.UserData == null)
                      {
                          continue;
                      }
                      var myRanking = result.Find ( x => x.Key == client.Id );

                      if ( client.userAgent.UserAssetData != null )
                      {
                          client.userAgent.UserAssetData.Tier = myRanking.Tier;
                      }
                      RankingEvent.OnUpdateRaidRanking ( client, myRanking, null, result );
                      BossEvent.OnUpdateRaidBoss ( client, 16 );
                  }
              } );

            UpdateBoss ( );
        }

        public void UpdateBoss()
        {
            TheLordServer.Log.Info ( "보스가 갱신이 시작되었습니다." );
            foreach ( var bossData in TheLordServer.Instance.bossDataList )
            {
                TheLordServer.Log.InfoFormat ( "{0}번 보스 갱신중 - {1}", bossData.Index, bossData.CreateTime );

                var workUpdateBoss = MongoHelper.BossCollection.Update ( bossData ).GetAwaiter ( );
                workUpdateBoss.OnCompleted ( ( ) =>
                  {

                  } );
            }
        }

        void UpdateTime()
        {
            try
            {
                var bossData = TheLordServer.Instance.bossDataList[0];
                var time = GameUtility.Now ( ) - bossData.CreateTime;
                var nextTime = bossRemainTime;
                if ( time >= nextTime )
                {
                    ResetRaid ( );
                }

                TimeSpan remainTime = updateBoss - GameUtility.Now ( );
                if(remainTime.Ticks <= 0)
                {
                    updateBoss = GameUtility.Now ( ) + updateTick;
                    UpdateBoss ( );
                }

            }
            catch(Exception e)
            {
                TheLordServer.Log.ErrorFormat ( "[UpdateTime] ",  e.Message );
            }
        }

    }
}
