using Photon.SocketServer;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TheLordServer.Handler
{
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Table.Structure;
    using Util;
    using Event;

    public class ContentHandler : Singleton<ContentHandler>, IBaseHandler
    {
        public void AddListener ( )
        {
            HandlerMedia.AddListener ( OperationCode.EnterContent, OnEnterContentReceived );
            HandlerMedia.AddListener ( OperationCode.EnterRaid, OnEnterRaidReceived );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.EnterContent, OnEnterContentReceived );
            HandlerMedia.RemoveListener ( OperationCode.EnterRaid, OnEnterRaidReceived );
        }

        void OnEnterContentReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            OperationResponse response = new OperationResponse ( (byte)OperationCode.EnterContent );

            if ( TheLordServer.Instance.bossDataList .Count == 0)
            {
                response.ReturnCode = (short)ReturnCode.Failed;
                peer.SendOperationResponse ( response, sendParameters );
                return;
            }
            var bossData = TheLordServer.Instance.bossDataList[0];

            response.ReturnCode = bossData.HP > 0 ? (short)ReturnCode.Success : (short)ReturnCode.Failed;

            if(response.ReturnCode == (short)ReturnCode.Failed)
            {
                BossEvent.OnUpdateRaidBoss ( peer, 16 );
            }

            peer.SendOperationResponse ( response, sendParameters );
        }

        void OnEnterRaidReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            OperationResponse response = new OperationResponse ( (byte)OperationCode.EnterRaid );

            if ( TheLordServer.Instance.bossDataList.Count == 0 || TheLordServer.Instance.bossDataList[0].HP <= 0F )
            {
                response.ReturnCode = (short)ReturnCode.Failed;
                peer.SendOperationResponse ( response, sendParameters );
                return;
            }
            var raidEnterData = new ProtoData.RaidEnterData ( );

            var bossData = TheLordServer.Instance.bossDataList[0];
            raidEnterData.raidBossData = new ProtoData.RaidBossData ( );
            raidEnterData.raidBossData.index = bossData.Index;
            raidEnterData.raidBossData.hp = (int)bossData.HP;

            foreach(var db in peer.userAgent.BuildingDataList)
            {
                var charactertData = new ProtoData.RaidEnterData.CharacterData ( );
                charactertData.index = db.CharactertData.Index;
                charactertData.amount = db.CharactertData.Amount;
                raidEnterData.characterDataList.Add ( charactertData );
            }

            var workScore = MongoHelper.RaidRankingCollection.Get ( peer.Id ).GetAwaiter ( );
            workScore.OnCompleted ( ( ) =>
              {
                  raidEnterData.totalScore = workScore.GetResult ( ).Score;
                  response.ReturnCode = (short)ReturnCode.Success;
                  response.Parameters = BinSerializer.ConvertPacket ( raidEnterData );
                  peer.SendOperationResponse ( response, sendParameters );
              } );
        }
    }
}
