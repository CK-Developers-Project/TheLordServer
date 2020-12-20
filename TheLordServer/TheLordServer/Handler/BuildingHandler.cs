﻿using System;
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

        private void ClickAction_BuildingBuild ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, byte operationCode, SendParameters sendParameters )
        {
            var workBuildingData = MongoHelper.BuildingCollection.GetByIndex ( peer.userData.Id, buildingClickData.index ).GetAwaiter ( );
            workBuildingData.OnCompleted ( ( ) =>
             {
                 var buildingData = workBuildingData.GetResult ( );

                 var sheet = TheLordTable.Instance.BuildingTable.BuildingInfoSheet;
                 var record = BaseTable.Get ( sheet, "index", buildingClickData.index );

                 int unitCreate = (int)record["unitCreate"];
                 int second = ( buildingData.LV + 1 ) * (int)record["buildTime"];

                 bool bCreate = buildingData == null;

                 if ( bCreate )
                 {
                     buildingData = new BuildingData ( peer.userData.Id );
                     buildingData.Index = buildingClickData.index;
                     buildingData.LV = 0;
                     buildingData.CharactertData.Index = unitCreate;
                 }

                 if ( buildingData.WorkTime.Ticks > 0 )
                 {
                     // TODO
                     // 이미 업글중임
                 }
                 else
                 {
                     buildingData.WorkTime = DateTime.Now + new TimeSpan ( 0, 0, second );

                     Action action = ( ) =>
                     {
                         OperationResponse response = new OperationResponse ( operationCode );
                         ProtoData.BuildingData packet = new ProtoData.BuildingData ( );
                         packet.index = buildingClickData.index;
                         packet.tick = buildingData.WorkTime.Ticks;

                         response.Parameters = BinSerializer.ConvertPacket ( packet );
                         peer.SendOperationResponse ( response, sendParameters );
                     };

                     if ( bCreate )
                     {
                         var workAdd = MongoHelper.BuildingCollection.Add ( buildingData ).GetAwaiter ( );
                         workAdd.OnCompleted ( action );
                     }
                     else
                     {
                         var workUpdate = MongoHelper.BuildingCollection.UpdateWorkTime ( buildingData ).GetAwaiter ( );
                         workUpdate.OnCompleted ( action );
                     }
                 }
             } );
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
                    ConfirmAction_Build ( );
                    break;
                case ConfirmAction.LevelUp:
                    ConfirmAction_LevelUp ( );
                    break;
                default:
                    Failed ( peer, sendParameters );
                    return;
            }
        }

        private void ConfirmAction_LevelUp ( )
        {
            throw new NotImplementedException ( );
        }

        private void ConfirmAction_Build ( )
        {
            throw new NotImplementedException ( );
        }
    }
}
