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
        const int Nickname_Min_Lenth = 3;
        const int Nickname_Max_Lenth = 6;

        enum NextAction : short
        {
            LoginSuccess = 0,   // 로그인 성공
            LoginFailed,        // 로그인 실패
            UserCreateFail,     // 유저생성 실패
            UserInfoCreate,     // 유저정보 생성
        }

        public void AddListener ( )
        {
            HandlerMedia.AddListener ( OperationCode.Login, OnLoginReceived );
            HandlerMedia.AddListener ( OperationCode.UserResistration, OnUserResistrationRecevied );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.Login, OnLoginReceived );
            HandlerMedia.RemoveListener ( OperationCode.UserResistration, OnUserResistrationRecevied );
        }

        public bool Create ( out UserData data, string id, string password )
        {
            data = new UserData ( id, password, DateTime.Now );
            try
            {
                TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} 유저가 생성되었습니다. ", id );
                MongoHelper.UserCollection.Add ( data );
            }
            catch ( Exception e )
            {
                TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} ", e.Message );
                return false;
            }
            return true;
        }

        void OnLoginReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.UserData user = BinSerializer.Deserialize<ProtoData.UserData> ( bytes );

            OperationResponse response = new OperationResponse ( operationRequest.OperationCode );
            UserData userData = MongoHelper.UserCollection.GetByUsername ( user.id );

            if ( userData == null )
            {
                if( user.id.Length < ID_Min_Lenth )
                {
                    response.ReturnCode = (short)NextAction.UserCreateFail;
                }
                else
                {
                    if ( Create ( out userData, user.id, user.password ) )
                    {
                        response.ReturnCode = (short)NextAction.UserInfoCreate;
                        peer.Id = userData.Id;
                    }
                    else
                    {
                        response.ReturnCode = (short)NextAction.UserCreateFail;
                    }
                }
            }
            else
            {
                bool bSuccess = MongoHelper.UserCollection.VerifyUser ( user.id, user.password );
                if (bSuccess)
                {
                    if ( userData.Info.Race == 0 || string.IsNullOrEmpty ( userData.Info.Nickname ) )
                    {
                        response.ReturnCode = (short)NextAction.UserInfoCreate;
                    }
                    else
                    {
                        response.ReturnCode = (short)ReturnCode.Success;
                        ProtoData.UserData packet = new ProtoData.UserData ( );
                        packet.nickname = userData.Info.Nickname;
                        packet.race = userData.Info.Race;
                        response.Parameters = BinSerializer.ConvertPacket ( packet );
                    }
                    peer.Id = userData.Id;
                }
                else
                {
                    response.ReturnCode = (short)NextAction.LoginFailed;
                }
            }

            peer.SendOperationResponse ( response, sendParameters );
        }

        void OnUserResistrationRecevied( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.UserData user = BinSerializer.Deserialize<ProtoData.UserData> ( bytes );

            OperationResponse response = new OperationResponse ( operationRequest.OperationCode );
            UserData userData = MongoHelper.UserCollection.Get ( peer.Id );

            if(null == userData)
            {
                response.ReturnCode = (short)ReturnCode.Failed;
            }
            else
            {
                int lenth = user.nickname.Length;
                if ( lenth < Nickname_Min_Lenth || lenth > Nickname_Max_Lenth || (user.race == 0 || user.race >= (int)Race.End) )
                {
                    response.ReturnCode = (short)ReturnCode.Failed;
                }
                else
                {
                    response.ReturnCode = (short)ReturnCode.Success;

                    userData.Info.Nickname = user.nickname;
                    userData.Info.Race = user.race;
                    MongoHelper.UserCollection.UpdateInfo ( userData );

                    ProtoData.UserData packet = new ProtoData.UserData ( );
                    packet.nickname = userData.Info.Nickname;
                    packet.race = userData.Info.Race;
                    response.Parameters = BinSerializer.ConvertPacket(packet);
                }
            }

            peer.SendOperationResponse ( response, sendParameters );
        }
    }
}
