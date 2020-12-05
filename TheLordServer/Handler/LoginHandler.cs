using Photon.SocketServer;
using System;
using System.Threading.Tasks;

namespace TheLordServer.Handler
{
    using MongoDB.CollectionData;
    using MongoDB.Model;
    using Table.Structure;
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

        public async Task<UserData> Create ( UserData data, string id, string password )
        {
            data = new UserData ( id, password, DateTime.Now );
            try
            {
                TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} 유저가 생성되었습니다. ", id );
                await MongoHelper.UserCollection.Add(data);
                return data;
            }
            catch ( Exception e )
            {
                TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} ", e.Message );
                return null;
            }
        }

        void OnLoginReceived( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.UserData user = BinSerializer.Deserialize<ProtoData.UserData> ( bytes );

            var workUserData = MongoHelper.UserCollection.GetByUsername(user.id).GetAwaiter();
            workUserData.OnCompleted(() =>
            {
                UserData userData = workUserData.GetResult();
                OperationResponse response = new OperationResponse(operationRequest.OperationCode);

                if (userData == null)
                {
                    if (user.id.Length < ID_Min_Lenth)
                    {
                        response.ReturnCode = (short)NextAction.UserCreateFail;
                        peer.SendOperationResponse(response, sendParameters);
                    }
                    else
                    {
                        var workCreate = Create(userData, user.id, user.password).GetAwaiter();
                        workCreate.OnCompleted(() =>
                        {
                            UserData result = workCreate.GetResult();
                            if(result == null)
                            {
                                response.ReturnCode = (short)NextAction.UserCreateFail;
                            }
                            else
                            {
                                response.ReturnCode = (short)NextAction.UserInfoCreate;
                                peer.Id = result.Id;
                            }
                            peer.SendOperationResponse(response, sendParameters);
                        });
                    }
                }
                else
                {
                    var workVerifyUser = MongoHelper.UserCollection.VerifyUser(user.id, user.password).GetAwaiter();
                    workVerifyUser.OnCompleted(() =>
                    {
                        bool bSuccess = workVerifyUser.GetResult();

                        if (bSuccess)
                        {
                            if (userData.Info.Race == 0 || string.IsNullOrEmpty(userData.Info.Nickname))
                            {
                                response.ReturnCode = (short)NextAction.UserInfoCreate;
                            }
                            else
                            {
                                response.ReturnCode = (short)ReturnCode.Success;
                                ProtoData.UserData packet = new ProtoData.UserData();
                                packet.nickname = userData.Info.Nickname;
                                packet.race = userData.Info.Race;
                                response.Parameters = BinSerializer.ConvertPacket(packet);
                            }
                            peer.Id = userData.Id;
                        }
                        else
                        {
                            response.ReturnCode = (short)NextAction.LoginFailed;
                        }
                        peer.SendOperationResponse(response, sendParameters);
                    });
                }
            });
        }

        void OnUserResistrationRecevied( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.UserData user = BinSerializer.Deserialize<ProtoData.UserData> ( bytes );

            OperationResponse response = new OperationResponse ( operationRequest.OperationCode );
            var workUserData = MongoHelper.UserCollection.Get(peer.Id).GetAwaiter();
            workUserData.OnCompleted(() => 
            {
                UserData userData = workUserData.GetResult();

                if (null == userData)
                {
                    response.ReturnCode = (short)ReturnCode.Failed;
                    peer.SendOperationResponse(response, sendParameters);
                }
                else
                {
                    int lenth = user.nickname.Length;
                    if (lenth < Nickname_Min_Lenth || lenth > Nickname_Max_Lenth || (user.race == 0 || user.race >= (int)Race.End))
                    {
                        response.ReturnCode = (short)ReturnCode.Failed;
                        peer.SendOperationResponse(response, sendParameters);
                    }
                    else
                    {
                        response.ReturnCode = (short)ReturnCode.Success;

                        userData.Info.Nickname = user.nickname;
                        userData.Info.Race = user.race;
                        var workUpdate = MongoHelper.UserCollection.UpdateInfo(userData).GetAwaiter();
                        workUpdate.OnCompleted(() =>
                        {
                            ProtoData.UserData packet = new ProtoData.UserData();
                            packet.nickname = userData.Info.Nickname;
                            packet.race = userData.Info.Race;
                            response.Parameters = BinSerializer.ConvertPacket(packet);
                            peer.SendOperationResponse(response, sendParameters);
                        });
                    }
                }
            });
        }
    }
}
