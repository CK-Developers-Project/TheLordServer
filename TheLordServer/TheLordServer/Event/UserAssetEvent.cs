using System;
using Photon.SocketServer;
using MongoDB.Bson;

namespace TheLordServer.Event
{
    using Util;
    using Table.Structure;
    using MongoDB.CollectionData;
    using MongoDB.Model;

    public class UserAssetEvent
    {
        public static void OnUpdateResource(ClientPeer peer)
        {
            TheLordServer.Log.InfoFormat ( "[OnUpdateResource] - {0}", peer.LocalIP );
            if ( peer.userAgent.UserData == null || peer.userAgent.UserAssetData == null)
            {
                // 로그인 씬으로
                return;
            }

            EventData data = new EventData ( (byte)EventCode.UpdateResource );
            var packet = new ProtoData.ResourceData ( );
            packet.gold = peer.userAgent.UserAssetData.Gold;
            packet.cash = peer.userAgent.UserAssetData.Cash;
            packet.index = peer.userAgent.UserAssetData.Index;
            packet.tier = peer.userAgent.UserAssetData.Tier;
            data.Parameters = BinSerializer.ConvertPacket ( packet );
            peer.SendEvent ( data, new SendParameters ( ) );
        }
    }
}
