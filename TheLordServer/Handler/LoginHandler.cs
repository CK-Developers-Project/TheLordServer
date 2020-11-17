using Photon.SocketServer;
using System;

namespace TheLordServer.Handler
{
    using MongoDB.Structure;
    using MongoDB.Model;
    using Structure;
    using Util;

    public class LoginHandler : Singleton<LoginHandler>, IBaseHandler
    {
        const int ID_Min_Lenth = 3;

        enum NextAction : short
        {
            LoginSuccess = 0,   // 로그인 성공
            LoginFailed,        // 로그인 실패
            UserCreateFail,     // 유저생성 실패
            UserInfoCreate,     // 유저정보 생성
        }

        public void AddListener ( )
        {
            TheLordServer.Log.Info ( " [LoginHandler.AddListener] " );
            HandlerMedia.AddListener ( OperationCode.Login, OnLoginReceived );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.Login, OnLoginReceived );
        }

        void OnLoginReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            TheLordServer.Log.Info ( " [LoginHandler.OnLoginReceived] 이벤트 발생 " );


            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoBuf2Data.UserID userID = BinSerializer.Deserialize<ProtoBuf2Data.UserID> ( bytes );

            OperationResponse response = new OperationResponse ( operationRequest.OperationCode );
            UserData data = MongoHelper.UserCollection.GetByUsername ( userID.Id );

            if ( data == null )
            {
                if(userID.Id.Length < ID_Min_Lenth )
                {
                    response.ReturnCode = (short)NextAction.UserCreateFail;
                }
                else
                {
                    if ( Create ( out data, userID.Id, userID.Password ) )
                    {
                        response.ReturnCode = (short)NextAction.UserInfoCreate;

                    }
                    else
                    {
                        response.ReturnCode = (short)NextAction.UserCreateFail;
                    }
                }
            }
            else
            {
                bool bSuccess = MongoHelper.UserCollection.VerifyUser ( userID.Id, userID.Password );
            
                if(bSuccess)
                {
                    response.ReturnCode = (short)ReturnCode.Success;
                    if ( data.Info.race == 0 || string.IsNullOrEmpty ( data.Info.nickname ) )
                    {
                        response.ReturnCode = (short)NextAction.UserInfoCreate;
                    }
                }
                else
                {
                    response.ReturnCode = (short)NextAction.LoginFailed;
                }
            }

            peer.SendOperationResponse ( response, sendParameters );
        }

        public bool Create ( out UserData data, string id, string password )
        {
            data = new UserData ( id, password, DateTime.Now );
            try
            {
                TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} 유저가 생성되었습니다. ", id );
                MongoHelper.UserCollection.Add ( data );
            }
            catch(Exception e)
            {
                TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} ", e.Message );
                return false;
            }
            return true;
        }
    }
}
