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
            var workUserAsset = MongoHelper.UserAssetCollection.Get ( peer.userData.Id ).GetAwaiter ( );
            workUserAsset.OnCompleted ( ( ) =>
             {
                 var userAsset = workUserAsset.GetResult ( );

                 if ( userAsset == null )
                 {
                     return;
                 }

                 EventData data = new EventData ( (byte)EventCode.UpdateResource );
                 ProtoData.ResourceData packet = new ProtoData.ResourceData ( );
                 packet.gold = userAsset.Gold + peer.userAgent.gold;
                 packet.cash = userAsset.Cash;
                 data.Parameters = BinSerializer.ConvertPacket ( packet );
                 peer.SendEvent ( data, new SendParameters ( ) );
             } );
        }
    }
}
