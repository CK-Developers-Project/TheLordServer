using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace TheLordServer
{
    using Table.Structure;
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Agent;

    public class ClientPeer : Photon.SocketServer.ClientPeer
    {
        public UserData userData {get; set;}
        public UserAgent userAgent = new UserAgent();

        public ClientPeer(InitRequest initRequest) : base(initRequest) { }

        protected override void OnDisconnect ( DisconnectReason reasonCode, string reasonDetail )
        {
            if ( userData != null )
            {
                var workUserAssetData = MongoHelper.UserAssetCollection.Get ( userData.Id ).GetAwaiter ( );
                workUserAssetData.OnCompleted ( ( ) =>
                {
                    var userAssetData = workUserAssetData.GetResult ( );
                    if(userAssetData == null)
                    {
                        return;
                    }
                    userAssetData.Resource.Gold = userAgent.gold.ToString ( );
                    var workUpdateGold = MongoHelper.UserAssetCollection.UpdateResource ( userAssetData ).GetAwaiter ( );
                    workUpdateGold.OnCompleted ( ( ) =>
                    {
                        TheLordServer.Log.InfoFormat ( "{0} 님의 골드가 [{1}] 갱신되었습니다.", userData.Info.Nickname, userAssetData.Gold );
                    } );
                } );
            }

            TheLordServer.Log.InfoFormat ( "[{0}]의 연결이 끊겼습니다.", userData.Id );
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
