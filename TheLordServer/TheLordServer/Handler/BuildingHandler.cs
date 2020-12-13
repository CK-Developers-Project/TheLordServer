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

    public class BuildingHandler : Singleton<BuildingHandler>, IBaseHandler
    {
        enum ClickAction : int
        {
            MainBuildingTakeGold,   // 골드를 받다.
        }


        public void AddListener ( )
        {
            HandlerMedia.AddListener ( OperationCode.BuildingClick, OnBuildingClickReceived );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.BuildingClick, OnBuildingClickReceived );
        }

        public void Failed ( ClientPeer peer, SendParameters sendParameters )
        {
            OperationResponse response = new OperationResponse ( );
            response.ReturnCode = (short)ReturnCode.Failed;
            peer.SendOperationResponse ( response, sendParameters );
        }

        void OnBuildingClickReceived(ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            var buildingClickData = BinSerializer.ConvertData<ProtoData.BuildingClickData> ( operationRequest.Parameters );
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
                if(buildingData == null)
                {
                    Failed ( peer, sendParameters );
                    return;
                }
                int increase = buildingData.LV * (int)record["nextLV"];
                BigInteger gold = new BigInteger ( increase * buildingClickData.value );
                peer.userAgent.gold += gold;
            } );
        }
    }
}
