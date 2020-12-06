using System;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;

    public class BuildingHandler : Singleton<UserAssetHandler>, IBaseHandler
    {
        enum ClickAction : int
        {
            TakeGold,   // 골드를 받다.
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
        
            
        }
    }
}
