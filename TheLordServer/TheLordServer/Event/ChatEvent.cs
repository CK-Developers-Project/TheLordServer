using System;
using System.Collections.Generic;
using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Util;
    using Table.Structure;

    public class ChatEvent
    {
        public static Queue<ProtoData.ChatData> chatDatas = new Queue<ProtoData.ChatData>();
        public static void OnUpdateChat()
        {
            while (true)
            {
                if (chatDatas.Count > 1)
                {
                    EventData data = new EventData((byte)EventCode.UpdateChat);
                    data.Parameters = BinSerializer.ConvertPacket(chatDatas.Dequeue());

                    foreach(ClientPeer peer in TheLordServer.Instance.peerList)
                        peer.SendEvent(data, new SendParameters());
                }
            }
        }
    }
}
