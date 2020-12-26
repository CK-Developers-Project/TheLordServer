using System;
using System.Collections.Generic;
using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Util;
    using Table.Structure;

    public class PeerChatData
    {
        public ClientPeer peer;
        public ProtoData.ChatData data;
    }

    public class ChatEvent
    {
        public static Queue<PeerChatData> chatDatas = new Queue<PeerChatData>();
        public static void OnUpdateChat()
        {
            while (true)
            {
                if (chatDatas.Count > 0)
                {
                    PeerChatData Peerdata = chatDatas.Dequeue();
                    ClientPeer myPeer = Peerdata.peer;
                    EventData data = new EventData((byte)EventCode.UpdateChat);
                    data.Parameters = BinSerializer.ConvertPacket(Peerdata.data);

                    foreach (ClientPeer peer in TheLordServer.Instance.peerList)
                    {
                        if (peer == myPeer)
                            continue;

                        peer.SendEvent(data, new SendParameters());
                    }
                }
            }
        }
    }
}
