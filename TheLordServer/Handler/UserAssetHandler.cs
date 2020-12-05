using System;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;

    public class UserAssetHandler : Singleton<UserAssetHandler>, IBaseHandler
    {
        public void AddListener()
        {
            HandlerMedia.AddListener(OperationCode.RequestResource, OnRequestResourceReceived);
        }

        public void RemoveListener()
        {
            HandlerMedia.RemoveListener(OperationCode.RequestResource, OnRequestResourceReceived);
        }


        void OnRequestResourceReceived(ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            OperationResponse response = new OperationResponse(operationRequest.OperationCode);
            
            peer.SendOperationResponse ( response, sendParameters );
        }
    }
}
