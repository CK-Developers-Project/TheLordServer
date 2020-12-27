using System;
using System.Collections.Generic;
using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Util;
    using Table.Structure;

    public class ChatEvent
    {
        public static void OnUpdateChat(ClientPeer peer, ProtoData.ChatData ChatData )
        {
            foreach(var client in TheLordServer.Instance.peerList)
            {
                if ( client.Equals(peer) )
                    continue;

                EventData data = new EventData ( (byte)EventCode.UpdateChat );
                data.Parameters = BinSerializer.ConvertPacket ( ChatData );
                client.SendEvent ( data, new SendParameters ( ) );
            }
        }
    }
}
