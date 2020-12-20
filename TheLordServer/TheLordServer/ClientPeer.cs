using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace TheLordServer
{
    using Table.Structure;
    using Agent;

    public class ClientPeer : Photon.SocketServer.ClientPeer
    {
        public UserAgent userAgent = new UserAgent();
        public ObjectId Id { get => userAgent.Id; }

        public ClientPeer(InitRequest initRequest) : base(initRequest) { }
        

        protected override void OnDisconnect ( DisconnectReason reasonCode, string reasonDetail )
        {
            if( userAgent.UserData != null)
            {
                Task.Run ( userAgent.Save );
            }

            TheLordServer.Log.InfoFormat ( "[{0}]의 연결이 끊겼습니다.", LocalIP );
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
