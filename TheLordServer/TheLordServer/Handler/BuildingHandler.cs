using System;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;
    using Table;
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Event;

    public class BuildingHandler : Singleton<UserAssetHandler>, IBaseHandler
    {
        enum ClickAction : int
        {
            MainBuildingTakeGold,   // 골드를 받다.
        }


        public void AddListener ( )
        {
            HandlerMedia.AddListener ( OperationCode.BuildingClick, OnBuildingClick );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.BuildingClick, OnBuildingClick );
        }

        public void Failed ( ClientPeer peer, SendParameters sendParameters )
        {
            OperationResponse response = new OperationResponse ( );
            response.ReturnCode = (short)ReturnCode.Failed;
            peer.SendOperationResponse ( response, sendParameters );
        }

        void OnBuildingClick(ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.BuildingClickData buildingClickData = BinSerializer.Deserialize<ProtoData.BuildingClickData> ( bytes );

            switch ( (ClickAction)buildingClickData.clickAction )
            {
                case ClickAction.MainBuildingTakeGold:
                    MainBuildingTakeGold ( peer, buildingClickData, sendParameters );
                    break;
                default:
                    Failed ( peer, sendParameters );
                    return;
            }
        }


        void MainBuildingTakeGold ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, SendParameters sendParameters )
        {
            var sheet = TheLordTable.Instance.BuildingTable.MainBuildingInfoSheet;
            var record = BaseTable.Get ( sheet, "index", buildingClickData.index );
            if ( null == record )
            {
                Failed ( peer, sendParameters );
                return;
            }

            var workBuildingData = MongoHelper.BuildingCollection.GetByIndex ( peer.userData.Id, buildingClickData.index ).GetAwaiter ( );
            workBuildingData.OnCompleted ( ( ) =>
            {
                var buildingData = workBuildingData.GetResult ( );
                int increase = ( buildingData.LV - 1 ) * (int)record["nextLV"];
                BigInteger gold = new BigInteger ( ( (int)record["basePoint"] + increase ) * buildingClickData.value );
                var workUserAssetData = MongoHelper.UserAssetCollection.Get ( peer.userData.Id ).GetAwaiter ( );
                workUserAssetData.OnCompleted ( ( ) =>
                {
                    var userAssetData = workUserAssetData.GetResult ( );
                    userAssetData.AddGold ( gold );
                    var workUpdateGold = MongoHelper.UserAssetCollection.UpdateGold ( userAssetData ).GetAwaiter ( );
                    workUpdateGold.OnCompleted ( ( ) =>
                    {
                        UserAssetEvent.OnUpdateResource ( peer );
                    } );
                } );
            } );
        }
    }
}
