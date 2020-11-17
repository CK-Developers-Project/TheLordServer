using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;

namespace TheLordServer
{
    using ExitGames.Logging;
    using Structure;

    public class ClientPeer : Photon.SocketServer.ClientPeer
    {
        public string username;
        public float x, y, z;

        public ClientPeer(InitRequest initRequest) : base(initRequest) { }

        protected override void OnDisconnect ( DisconnectReason reasonCode, string reasonDetail )
        {
            TheLordServer.Instance.peerList.Remove(this);
        }

        // [tooltip]
        // 클라이언트로부터 요청 처리
        // operationRequest : 요청 된 정보를 캡슐화 합니다.
        // sendParameters : 클라이언트에서 서버로 전달되는 데이터
        protected override void OnOperationRequest ( OperationRequest operationRequest, SendParameters sendParameters )
        {
            HandlerMedia.Dispatch ( (OperationCode)operationRequest.OperationCode, this, operationRequest, sendParameters );
        }
    }
}
