using Photon.SocketServer;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            HandlerMedia.AddListener ( OperationCode.LobbyEnter, OnLobbyEnterRecevied );
        }

        public void RemoveListener ( )
        {
            HandlerMedia.RemoveListener ( OperationCode.Login, OnLoginReceived );
            HandlerMedia.RemoveListener ( OperationCode.UserResistration, OnUserResistrationRecevied );
            HandlerMedia.RemoveListener ( OperationCode.LobbyEnter, OnLobbyEnterRecevied );
        }

        #region Async Function - 내부 함수
        /// <summary>
        /// 유저를 생성하고 DB(UserData)에 정보를 저장합니다.
        /// </summary>
        public async Task<UserData> Create ( UserData data, string id, string password )
        {
            data = new UserData ( id, password, DateTime.Now );
            TheLordServer.Log.InfoFormat ( " [LoginHandler.Create] - {0} 유저가 생성되었습니다. ", id );
            await MongoHelper.UserCollection.Add ( data );
            return data;
        }

        /// <summary>
        /// 유저의 첫 설정이 끝났다면 필요한 정보들을 초기화 하고 DB에 저장합니다.
        /// </summary>
        public async Task UserInitialize ( UserData userData )
        {
            var userAssetData = new UserAssetData ( userData.Id );
            var userAssetDataTask = MongoHelper.UserAssetCollection.Add ( userAssetData );

            var buildingData = new BuildingData ( userData.Id );
            buildingData.LV = 1;
            switch ( (Race)userData.Info.Race )
            {
                case Race.Elf:
                    buildingData.Index = 1;
                    break;
                case Race.Human:
                    buildingData.Index = 101;
                    break;
                case Race.Undead:
                    buildingData.Index = 201;
                    break;
                default:
                    // [TODO] 에러 보내야함
                    return;
            }
            var buildingDataTask = MongoHelper.BuildingCollection.Add ( buildingData );

            await Task.WhenAll (
                new List<Task> 
                { 
                    userAssetDataTask, 
                    buildingDataTask 
                }
            );
        }

        async Task<Dictionary<byte, object>> DBLoad ( ClientPeer peer )
        {
            List<Task> work = new List<Task> ( );

            var userAssetData = await MongoHelper.UserAssetCollection.Get ( peer.userData.Id );
            if ( userAssetData == null )
            {
                userAssetData = new UserAssetData ( peer.userData.Id );
                work.Add ( MongoHelper.UserAssetCollection.Add ( userAssetData ) );
            }
            peer.userAgent.gold = new BigInteger ( userAssetData.Gold );

            int buildingIndex;
            switch ( (Race)peer.userData.Info.Race )
            {
                case Race.Elf:
                    buildingIndex = 1;
                    break;
                case Race.Human:
                    buildingIndex = 101;
                    break;
                case Race.Undead:
                    buildingIndex = 201;
                    break;
                default:
                    // TODO 에러 보냄
                    return null;
            }

            var buildingData = await MongoHelper.BuildingCollection.GetByIndex ( peer.userData.Id, buildingIndex );
            if(buildingData == null)
            {
                buildingData = new BuildingData ( peer.userData.Id );
                buildingData.LV = 1;
                buildingData.Index = buildingIndex;
                work.Add ( MongoHelper.BuildingCollection.Add ( buildingData ) );
            }

            if ( work.Count > 0 )
            {
                await Task.WhenAll ( work );
            }

            var buildingListData = await MongoHelper.BuildingCollection.GetAll ( peer.userData.Id );

            ProtoData.DBLoadData DBLoadData = new ProtoData.DBLoadData ( );
            DBLoadData.resourceData = new ProtoData.ResourceData ( );
            DBLoadData.resourceData.gold = userAssetData.Gold;
            DBLoadData.resourceData.cash = userAssetData.Cash;
            foreach(var data in buildingListData)
            {
                ProtoData.DBLoadData.BuildingData bd = new ProtoData.DBLoadData.BuildingData ( );
                bd.index = data.Index;
                bd.LV = data.LV;
                bd.tick = data.WorkTime.Ticks;
                DBLoadData.buildingDataList.Add ( bd );
            }
            return BinSerializer.ConvertPacket ( DBLoadData );
        }
        #endregion


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
                                peer.userData = result;
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
                            peer.userData = userData;
                            if (userData.Info.Race == 0 || string.IsNullOrEmpty(userData.Info.Nickname))
                            {
                                response.ReturnCode = (short)NextAction.UserInfoCreate;
                                peer.SendOperationResponse ( response, sendParameters );
                            }
                            else
                            {
                                var wokrUserInitialize = UserInitialize ( userData ).GetAwaiter ( );
                                wokrUserInitialize.OnCompleted ( ( ) =>
                                {
                                    response.ReturnCode = (short)NextAction.LoginSuccess;
                                    ProtoData.UserData packet = new ProtoData.UserData ( );
                                    packet.nickname = userData.Info.Nickname;
                                    packet.race = userData.Info.Race;
                                    response.Parameters = BinSerializer.ConvertPacket ( packet );
                                    peer.SendOperationResponse ( response, sendParameters );
                                } );
                            }
                        }
                        else
                        {
                            response.ReturnCode = (short)NextAction.LoginFailed;
                            peer.SendOperationResponse ( response, sendParameters );
                        }
                    });
                }
            });
        }

        void OnUserResistrationRecevied( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.UserData user = BinSerializer.Deserialize<ProtoData.UserData> ( bytes );

            OperationResponse response = new OperationResponse ( operationRequest.OperationCode );
            var workUserData = MongoHelper.UserCollection.Get(peer.userData.Id).GetAwaiter();
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

        void OnLobbyEnterRecevied ( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            // [Tooltip] 검증과정
            var workDBLoad = DBLoad ( peer ).GetAwaiter ( );
            workDBLoad.OnCompleted ( ( ) =>
            {
                var DBLoadData = workDBLoad.GetResult ( );
                OperationResponse response = new OperationResponse ( operationRequest.OperationCode );

                if (DBLoadData == null)
                {
                    response.ReturnCode = (short)ReturnCode.Failed;
                }
                else
                {
                    response.ReturnCode = (short)ReturnCode.Success;
                    response.Parameters = DBLoadData;
                }
                
                peer.SendOperationResponse ( response, sendParameters );
            } );
        }
    }
}
