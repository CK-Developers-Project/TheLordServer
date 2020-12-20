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
            BuildingBuild,          // 건물을 짓다.
            BuildingLevelUp,        // 건물 레벨업
            CharacterHire,          // 캐릭터를 고용
        }

        public enum ConfirmAction : int
        {
            Build,                  // 건물 지음 확인
            LevelUp,                // 건물 레벨업
        }

        public void AddListener ( )
        {
            HandlerMedia.AddListener ( OperationCode.BuildingClick, OnBuildingClickReceived );
            HandlerMedia.AddListener ( OperationCode.BuildingConfirm, OnBuildingConfirmReceived );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.BuildingClick, OnBuildingClickReceived );
            HandlerMedia.RemoveListener ( OperationCode.BuildingConfirm, OnBuildingConfirmReceived );
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
                    ClickAction_MainBuildingTakeGold ( peer, buildingClickData, sendParameters );
                    break;
                case ClickAction.BuildingBuild:
                    ClickAction_BuildingBuild ( peer, buildingClickData, operationRequest.OperationCode, sendParameters );
                    break;
                case ClickAction.BuildingLevelUp:
                    ClickAction_BuildingLevelUp ( peer, buildingClickData, sendParameters );
                    break;
                case ClickAction.CharacterHire:
                    CkickAction_CharacterHire ( peer, buildingClickData, sendParameters );
                    break;
                default:
                    Failed ( peer, sendParameters );
                    return;
            }
        }

        void ClickAction_MainBuildingTakeGold ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, SendParameters sendParameters )
        {
            if(peer.userAgent.UserData == null)
            {
                Failed ( peer, sendParameters );
                return;
            }

            int index = buildingClickData.index;
            var sheet = TheLordTable.Instance.BuildingTable.MainBuildingInfoSheet;
            var record = BaseTable.Get ( sheet, "index", index );
            if ( null == record )
            {
                Failed ( peer, sendParameters );
                return;
            }

            var buildingData = peer.userAgent.BuildingDataList.Find ( x => x.Index == index );
            if ( buildingData == null )
            {
                Failed ( peer, sendParameters );
                return;
            }

            int increase = buildingData.LV * (int)record["nextLV"];
            BigInteger gold = new BigInteger ( increase * buildingClickData.value );
            peer.userAgent.UserAssetData.AddGold ( gold );
        }

        private void ClickAction_BuildingBuild ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, byte operationCode, SendParameters sendParameters )
        {
            int index = buildingClickData.index;

            var buildingData = peer.userAgent.BuildingDataList.Find ( x => x.Index == index );
            var response = new OperationResponse(operationCode);

            var sheet = TheLordTable.Instance.BuildingTable.BuildingInfoSheet;
            var record = BaseTable.Get ( sheet, "index", index );

            int unitCreate = (int)record["unitCreate"];
            int second = ( buildingData.LV + 1 ) * (int)record["buildTime"];

            if ( buildingData == null )
            {
                buildingData = new BuildingData ( peer.Id );
                buildingData.Index = buildingClickData.index;
                buildingData.LV = 0;
                buildingData.CharactertData.Index = unitCreate;
            }

            if ( buildingData.WorkTime.Ticks > 0 )
            {
                // 이미 업글중 예외 처리
                response.ReturnCode = (short)ReturnCode.Failed;
            }
            else
            {
                buildingData.WorkTime = DateTime.Now + new TimeSpan ( 0, 0, second );
                ProtoData.BuildingData packet = new ProtoData.BuildingData ( );
                packet.index = buildingClickData.index;
                packet.tick = buildingData.WorkTime.Ticks;

                response.Parameters = BinSerializer.ConvertPacket ( packet );
                response.ReturnCode = (short)ReturnCode.Success;
                peer.SendOperationResponse ( response, sendParameters );
            }
        }

        private void ClickAction_BuildingLevelUp ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, SendParameters sendParameters )
        {
            throw new NotImplementedException ( );
        }
        
        private void CkickAction_CharacterHire ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, SendParameters sendParameters )
        {
            throw new NotImplementedException ( );
        }


        void OnBuildingConfirmReceived ( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            var buildingConfirmData = BinSerializer.ConvertData<ProtoData.BuildingConfirmData> ( operationRequest.Parameters );
            switch ( (ConfirmAction)buildingConfirmData.confirmAction )
            {
                case ConfirmAction.Build:
                    ConfirmAction_Build (peer, operationRequest, sendParameters, buildingConfirmData);
                    break;
                case ConfirmAction.LevelUp:
                    ConfirmAction_LevelUp ( peer, operationRequest, sendParameters, buildingConfirmData);
                    break;
                default:
                    Failed ( peer, sendParameters );
                    return;
            }
        }

        private void ConfirmAction_Build(ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters, ProtoData.BuildingConfirmData buildingConfirmData)
        {
            int index = buildingConfirmData.index;
            var buildingData = peer.userAgent.BuildingDataList.Find(x => x.Index == index);
            OperationResponse response = new OperationResponse(operationRequest.OperationCode);

            if (buildingData == null)
            {
                // 건물이 없음 예외처리
            }
            else
            {
                if (buildingData.WorkTime.Ticks <= 0)
                {
                    buildingData.LV = 1;
                    buildingData.WorkTime = default;
                    response.ReturnCode = (short)ReturnCode.Success;
                }
                else
                {
                    // 시간 예외처리
                }
            }

            peer.SendOperationResponse(response, sendParameters);
        }

        private void ConfirmAction_LevelUp (ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters, ProtoData.BuildingConfirmData buildingConfirmData)
        {
            int index = buildingConfirmData.index;
            var buildingData = peer.userAgent.BuildingDataList.Find(x => x.Index == index);
            OperationResponse response = new OperationResponse(operationRequest.OperationCode);

            if (buildingData == null)
            {
                // 건물이 없음 예외처리
            }
            else
            {
                if (buildingData.WorkTime.Ticks <= 0)
                {
                    buildingData.LV++;
                    buildingData.WorkTime = default;
                    response.ReturnCode = (short)ReturnCode.Success;
                }
                else
                {
                    // 시간 예외처리
                }
            }

            peer.SendOperationResponse(response, sendParameters);
        }
    }
}
