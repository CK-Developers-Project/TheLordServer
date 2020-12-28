using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TheLordServer.Agent
{
    using Util;
    using MongoDB.CollectionData;
    using MongoDB.Model;

    public class UserAgent
    {
        public UserData UserData { get; set; }
        public UserAssetData UserAssetData { get; set; }
        public List<BuildingData> BuildingDataList { get; set; }

        public ObjectId Id { get => UserData.Id; }

        public bool isLoad = false;

        public async Task Load()
        {
            if(isLoad)
            {
                return;
            }

            isLoad = true;
            UserAssetData = await MongoHelper.UserAssetCollection.Get ( Id );
            BuildingDataList = await MongoHelper.BuildingCollection.GetAll ( Id );

            var rankingData = await MongoHelper.RaidRankingCollection.Get ( Id );
            if( rankingData  != null)
            {
                UserAssetData.Tier = rankingData.Tier;
            }
        }


        public async void Save ()
        {
            try
            {
                if ( UserAssetData != null )
                {
                    await MongoHelper.UserAssetCollection.Update ( UserAssetData );
                }
                
                if(BuildingDataList != null)
                {
                    List<Task> workBuildingDataList = new List<Task> ( );
                    foreach ( var bd in BuildingDataList )
                    {
                        workBuildingDataList.Add ( MongoHelper.BuildingCollection.Update ( bd ) );
                    }
                    await Task.WhenAll ( workBuildingDataList );
                }

                TheLordServer.Log.InfoFormat ( "{0}의 정보를 동기화 하였습니다.", UserData.Info.Nickname );
            }
            catch ( MongoException e )
            {
                TheLordServer.Log.ErrorFormat ( "[UserAgent.Save] {0}", e.Message );
            }
            catch (Exception e)
            {
                TheLordServer.Log.ErrorFormat ( "[UserAgent.Save] {0}", e.Message );
            }
            
        }

    }
}
