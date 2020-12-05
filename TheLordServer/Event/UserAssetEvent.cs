using System;
using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Table.Structure;

    public class UserAssetEvent
    {

        public void OnUpdateResource(ClientPeer peer)
        {
            EventData data = new EventData((byte)EventCode.UpdateResource);
            peer.SendEvent (data, new SendParameters ( ) );
        }
    }
}
