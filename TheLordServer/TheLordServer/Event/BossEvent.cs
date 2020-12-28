using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Table.Structure;
    using Util;
    using MongoDB.CollectionData;
    using MongoDB.Model;

    public class BossEvent
    {
        public static void OnUpdateRaidBoss(ClientPeer peer, int index)
        {
            EventData eventData = new EventData ( (byte)EventCode.UpdateRaidBoss );

            var bossData = TheLordServer.Instance.bossDataList[0];
            if(bossData == null)
            {
                TheLordServer.Log.Info ( "보스가 없습니다." );
                return;
            }
            
            var packet = new ProtoData.RaidBossData ( );
            packet.index = bossData.Index;
            packet.hp = (int)bossData.HP;
            eventData.Parameters = BinSerializer.ConvertPacket ( packet );
            peer.SendEvent ( eventData, new SendParameters ( ) );
        }

        public static void OnExitRaidBoss( )
        {
            EventData eventData = new EventData ( (byte)EventCode.ExitRaidBoss );
            foreach (var client in TheLordServer.Instance.PeerList )
            {
                client.SendEvent ( eventData, new SendParameters ( ) );
            }
        }
    }
}
