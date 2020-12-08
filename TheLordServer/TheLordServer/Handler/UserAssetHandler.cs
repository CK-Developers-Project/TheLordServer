using System;
using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Util;
    using Table.Structure;
    using Event;

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


        void OnCheckUserInfoReceived ( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {

        }

        void OnRequestResourceReceived ( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            UserAssetEvent.OnUpdateResource ( peer );
        }
    }
}
