using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

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


        public async Task Load()
        {
            UserAssetData = await MongoHelper.UserAssetCollection.Get ( Id );
            BuildingDataList = await MongoHelper.BuildingCollection.GetAll ( Id );
        }


        public async Task Save ()
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
            catch (Exception e)
            {
                TheLordServer.Log.ErrorFormat ( "[UserAgent.Save] {0}", e.Message );
            }
        }

    }
}
