using System;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;
    using Table;

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
                    // TODO : 오류 반환
                    break;
            }
        }


        void MainBuildingTakeGold( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, SendParameters sendParameters )
        {
            var sheet = TheLordTable.Instance.BuildingTable.MainBuildingInfoSheet;
            var record = BaseTable.Get ( sheet, "index", buildingClickData.index );
            if ( null == record )
            {
                // TODO : 오류 반환
            }
            //int gold = (int)record["basePoint"] + ();
        }
    }
}
