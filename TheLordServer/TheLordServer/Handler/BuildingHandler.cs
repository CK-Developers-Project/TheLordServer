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
                    ClickAction_BuildingLevelUp ( peer, buildingClickData, operationRequest.OperationCode, sendParameters);
                    break;
                case ClickAction.CharacterHire:
                    CkickAction_CharacterHire ( peer, buildingClickData, operationRequest.OperationCode, sendParameters );
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

            int cost = (int)record["cost"];

            if (peer.userAgent.UserAssetData.GetGold() < cost)
            {
                // 돈 부족 예외처리
                response.ReturnCode = (short)ReturnCode.Failed;
                response.Parameters = BinSerializer.ConvertPacket(buildingClickData);
            }
            else
            {
                if (buildingData == null)
                {
                    buildingData = new BuildingData(peer.Id);
                    buildingData.Index = index;
                    buildingData.LV = 0;
                    buildingData.CharactertData.Index = unitCreate;
                    peer.userAgent.BuildingDataList.Add(buildingData);
                }

                if (buildingData.WorkTime.Ticks > 0)
                {
                    // 이미 업글중 예외 처리
                    var packet = new ProtoData.BuildingClickData();
                    packet.index = index;
                    packet.clickAction = buildingClickData.clickAction;

                    response.ReturnCode = (short)ReturnCode.Success;
                    response.Parameters = BinSerializer.ConvertPacket(packet);
                }
                else
                {
                    int second = (buildingData.LV + 1) * (int)record["buildTime"];
                    buildingData.WorkTime = DateTime.UtcNow.ToUniversalTime ( ) + new TimeSpan(0, 0, second);

                    var packet = new ProtoData.BuildingClickData();
                    packet.index = index;
                    packet.clickAction = buildingClickData.clickAction;

                    response.ReturnCode = (short)ReturnCode.Success;
                    response.Parameters = BinSerializer.ConvertPacket(packet);

                    BigInteger gold = new BigInteger(cost);
                    peer.userAgent.UserAssetData.AddGold(-gold);

                }
            }
            UserAssetEvent.OnUpdateResource(peer);
            BuildingEvent.OnUpdateBuilding(peer, index);
            peer.SendOperationResponse(response, sendParameters);
        }

        private void ClickAction_BuildingLevelUp ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, byte operationCode, SendParameters sendParameters )
        {
            int index = buildingClickData.index;

            var buildingData = peer.userAgent.BuildingDataList.Find(x => x.Index == index);
            var response = new OperationResponse(operationCode);

            var sheet = TheLordTable.Instance.BuildingTable.BuildingInfoSheet;
            var costSheet = TheLordTable.Instance.BuildingTable.BuildCostSheet;
            var record = BaseTable.Get(sheet, "index", index);

            if (buildingData == null)
            {
                // 건물 없음 예외처리
                response.ReturnCode = (short)ReturnCode.Failed;
                response.Parameters = BinSerializer.ConvertPacket(buildingClickData);
            }
            else
            {
                int buildingIndex = 0;

                switch ((Race)peer.userAgent.UserData.Info.Race)
                {
                    case Race.Elf:
                        buildingIndex = 1;
                        break;
                    case Race.Human:
                        buildingIndex = 101;
                        break;
                    case Race.Undead:
                        buildingIndex = 201;
                        break;
                }

                bool isMain = buildingIndex == buildingData.Index;
                float costRate = 0;
                float timeRate = 0; 

                foreach (var st in costSheet)
                {
                    if ((int)st["LV"] > buildingData.LV)
                    {
                        break;
                    }
                    costRate = isMain ? (float)st["mainBuildingRate"] : (float)st["NormalRate"];
                    timeRate = (float)st["buildTimeRate"];
                }
                int cost = (int)(buildingData.LV * (int)record["nextLV"] * costRate);

                if (peer.userAgent.UserAssetData.GetGold() < cost)
                {
                    // 돈 부족 예외처리
                    response.ReturnCode = (short)ReturnCode.Failed;
                    response.Parameters = BinSerializer.ConvertPacket(buildingClickData);
                }
                else
                {
                    if (buildingData.WorkTime.Ticks > 0)
                    {
                        // 이미 업글중 예외 처리
                        var packet = new ProtoData.BuildingClickData();
                        packet.index = index;
                        packet.clickAction = buildingClickData.clickAction;

                        response.ReturnCode = (short)ReturnCode.Success;
                        response.Parameters = BinSerializer.ConvertPacket(packet);
                    }
                    else
                    {
                        int second = (buildingData.LV + 1) * (int)((int)record["buildTime"] * timeRate);
                        buildingData.WorkTime = DateTime.UtcNow.ToUniversalTime ( ) + new TimeSpan(0, 0, second);

                        var packet = new ProtoData.BuildingClickData();
                        packet.index = index;
                        packet.clickAction = buildingClickData.clickAction;

                        response.ReturnCode = (short)ReturnCode.Success;
                        response.Parameters = BinSerializer.ConvertPacket(packet);

                        BigInteger gold = new BigInteger(cost);
                        peer.userAgent.UserAssetData.AddGold(-gold);

                    }
                }
            }

            UserAssetEvent.OnUpdateResource(peer);
            BuildingEvent.OnUpdateBuilding(peer, index);
            peer.SendOperationResponse(response, sendParameters);
        }
        
        private void CkickAction_CharacterHire ( ClientPeer peer, ProtoData.BuildingClickData buildingClickData, byte operationCode, SendParameters sendParameters )
        {
            int index = buildingClickData.index;

            var buildingData = peer.userAgent.BuildingDataList.Find(x => x.Index == index);
            var response = new OperationResponse(operationCode);

            if (buildingData == null)
            {
                // 건물 없음 예외처리
                response.ReturnCode = (short)ReturnCode.Failed;
                response.Parameters = BinSerializer.ConvertPacket(buildingClickData);
            }

            var buildingInfoSheet = TheLordTable.Instance.BuildingTable.BuildingInfoSheet;
            var buildingInfoRecord = BaseTable.Get(buildingInfoSheet, "index", index);
            int unitCreate = (int)buildingInfoRecord["unitCreate"];

            var charactertInfoSheet = TheLordTable.Instance.CharacterTable.CharacterInfoSheet;
            var charactertInfoRecord = BaseTable.Get(charactertInfoSheet, "index", index);
            
            int hireCost = (int)charactertInfoRecord["cost"];
            int hireCount = buildingData.LV * unitCreate;

            if (peer.userAgent.UserAssetData.GetGold() < hireCost)
            {
                // 돈 부족 예외처리
                response.ReturnCode = (short)ReturnCode.Failed;
                response.Parameters = BinSerializer.ConvertPacket(buildingClickData);
            }
            else
            {
                var packet = new ProtoData.BuildingClickData();
                packet.index = index;
                packet.clickAction = buildingClickData.clickAction;

                response.ReturnCode = (short)ReturnCode.Success;
                response.Parameters = BinSerializer.ConvertPacket(packet);

                // 후처리
                BigInteger gold = new BigInteger(hireCost);
                peer.userAgent.UserAssetData.AddGold(-gold);

                buildingData.CharactertData.Amount += hireCount;
            }

            UserAssetEvent.OnUpdateResource(peer);
            BuildingEvent.OnUpdateBuilding(peer, index);
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
                response.ReturnCode = (short)ReturnCode.Failed;
            }
            else
            {
                bool bNotWork = buildingData.WorkTime.Equals(default);
                if (bNotWork)
                {
                    // 건물이 지어지고 있는 상태가 아님
                    response.ReturnCode = (short)ReturnCode.Failed;
                }
                else
                {
                    TimeSpan targetTime = buildingData.WorkTime - DateTime.UtcNow.ToUniversalTime();

                    if (targetTime.TotalSeconds <= 0)
                    {
                        buildingData.LV++;
                        buildingData.WorkTime = default;
                        response.ReturnCode = (short)ReturnCode.Success;
                    }
                    else
                    {
                        // 시간 예외처리
                        response.ReturnCode = (short)ReturnCode.Failed;
                    }
                }
            }

            BuildingEvent.OnUpdateBuilding(peer, index);
            response.Parameters = BinSerializer.ConvertPacket(buildingConfirmData);
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
                response.ReturnCode = (short)ReturnCode.Failed;
            }
            else
            {
                bool bNotWork = buildingData.WorkTime.Equals(default);
                if(bNotWork)
                {
                    // 건물이 지어지고 있는 상태가 아님
                    response.ReturnCode = (short)ReturnCode.Failed;
                }
                else
                {
                    TimeSpan targetTime = buildingData.WorkTime - DateTime.UtcNow.ToUniversalTime();

                    if (targetTime.TotalSeconds <= 0)
                    {
                        buildingData.LV++;
                        buildingData.WorkTime = default;
                        response.ReturnCode = (short)ReturnCode.Success;
                    }
                    else
                    {
                        // 시간 예외처리
                        response.ReturnCode = (short)ReturnCode.Failed;
                    }
                }
            }

            BuildingEvent.OnUpdateBuilding(peer, index);
            response.Parameters = BinSerializer.ConvertPacket(buildingConfirmData);
            peer.SendOperationResponse(response, sendParameters);
        }
    }
}
