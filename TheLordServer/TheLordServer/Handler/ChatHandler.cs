using System;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;
    using Table;
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Event;

    public class ChatHandler : Singleton<ChatHandler>, IBaseHandler
    {
        public void AddListener()
        {
            HandlerMedia.AddListener(OperationCode.Chat, OnChatReceived);
        }

        public void RemoveListener()
        {
            HandlerMedia.RemoveListener(OperationCode.Chat, OnChatReceived);
        }

        void OnChatReceived(ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            var chatData = BinSerializer.ConvertData<ProtoData.ChatData>(operationRequest.Parameters);
            PeerChatData peerData = new PeerChatData();
            peerData.peer = peer;
            peerData.data = chatData;

            ChatEvent.chatDatas.Enqueue(peerData);
        }

        public void Failed(ClientPeer peer, SendParameters sendParameters)
        {
            OperationResponse response = new OperationResponse();
            response.ReturnCode = (short)ReturnCode.Failed;
            peer.SendOperationResponse(response, sendParameters);
        }
    }
}
