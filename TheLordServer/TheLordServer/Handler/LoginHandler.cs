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

        async Task<Dictionary<byte, object>> DBLoad ( ClientPeer peer )
        {
            await peer.userAgent.Load ( );

            if ( peer.userAgent.UserAssetData == null )
            {
                peer.userAgent.UserAssetData = new UserAssetData ( peer.Id );
            }

            if(peer.userAgent.BuildingDataList == null)
            {
                peer.userAgent.BuildingDataList = new List<BuildingData> ( );
            }

            int buildingIndex = 0;
            switch ( (Race)peer.userAgent.UserData.Info.Race )
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
                    // TODO 에러보냄
                    TheLordServer.Log.ErrorFormat ( "[{0}]의 종족값이 이상합니다.", peer.Id );
                    return null;
            }

            bool bExistMainBuilding = peer.userAgent.BuildingDataList.Exists ( x => x.Index == buildingIndex );
            if ( bExistMainBuilding )
            {
                var buildingData = new BuildingData ( peer.Id );
                buildingData.Index = buildingIndex;
                buildingData.LV = 1;
                peer.userAgent.BuildingDataList.Add ( buildingData );
            }

            ProtoData.DBLoadData DBLoadData = new ProtoData.DBLoadData ( );
            DBLoadData.resourceData = new ProtoData.ResourceData ( );
            DBLoadData.resourceData.gold = peer.userAgent.UserAssetData.Gold;
            DBLoadData.resourceData.cash = peer.userAgent.UserAssetData.Cash;
            foreach ( var data in peer.userAgent.BuildingDataList )
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
                                peer.userAgent.UserData = result;
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
                            peer.userAgent.UserData = userData;
                            if (userData.Info.Race == 0 || string.IsNullOrEmpty(userData.Info.Nickname))
                            {
                                response.ReturnCode = (short)NextAction.UserInfoCreate;
                            }
                            else
                            {
                                response.ReturnCode = (short)NextAction.LoginSuccess;
                                ProtoData.UserData packet = new ProtoData.UserData ( );
                                packet.nickname = userData.Info.Nickname;
                                packet.race = userData.Info.Race;
                                response.Parameters = BinSerializer.ConvertPacket ( packet );
                            }
                            peer.SendOperationResponse ( response, sendParameters );

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
            OperationResponse response = new OperationResponse ( operationRequest.OperationCode );

            if(peer.userAgent.UserData == null)
            {
                // 로그인 씬으로
                return;
            }
            
            byte[] bytes = (byte[])DictionaryTool.GetValue<byte, object> ( operationRequest.Parameters, 1 );
            ProtoData.UserData user = BinSerializer.Deserialize<ProtoData.UserData> ( bytes );

            int lenth = user.nickname.Length;

            if ( lenth < Nickname_Min_Lenth || lenth > Nickname_Max_Lenth || ( user.race == 0 || user.race >= (int)Race.End ) )
            {
                response.ReturnCode = (short)ReturnCode.Failed;
                peer.SendOperationResponse ( response, sendParameters );
            }
            else
            {
                response.ReturnCode = (short)ReturnCode.Success;

                peer.userAgent.UserData.Info.Nickname = user.nickname;
                peer.userAgent.UserData.Info.Race = user.race;

                ProtoData.UserData packet = new ProtoData.UserData ( );
                packet.nickname = peer.userAgent.UserData.Info.Nickname;
                packet.race = peer.userAgent.UserData.Info.Race;
                response.Parameters = BinSerializer.ConvertPacket ( packet );
                peer.SendOperationResponse ( response, sendParameters );
            }
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
