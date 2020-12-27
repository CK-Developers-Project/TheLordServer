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

            if (chatData.msg.IndexOf("/gold ") == 0)
            {
                BigInteger gold = int.Parse(chatData.msg.Replace("/gold ", ""));
                peer.userAgent.UserAssetData.AddGold(gold);
                UserAssetEvent.OnUpdateResource(peer);
                return;
            }

            ChatEvent.OnUpdateChat ( peer, chatData );
        }

        public void Failed(ClientPeer peer, SendParameters sendParameters)
        {
            OperationResponse response = new OperationResponse();
            response.ReturnCode = (short)ReturnCode.Failed;
            peer.SendOperationResponse(response, sendParameters);
        }
    }
}
