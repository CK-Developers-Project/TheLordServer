using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using Photon.SocketServer;

namespace TheLordServer
{
    using Table;
    using Table.Structure;
    using Handler;
    using MongoDB.Model;

    // [Tootip]
    // 모든 서버의 기본 클래스는 ApplicationBase에 상속되어야 합니다
    public class TheLordServer : ApplicationBase
    {

        public static readonly ILogger Log = LogManager.GetCurrentClassLogger ( );
        public static new TheLordServer Instance { get; private set; }

        public List<ClientPeer> peerList = new List<ClientPeer>();

        // [tooltip]
        // 클라이언트가 연결을 요청하면 서버가 해당 메소드를 호출합니다
        // PeerBase를 사용하여 클라이언트와의 링크를 나타내면 Photon이 이러한 링크를 관리합니다
        protected override PeerBase CreatePeer ( InitRequest initRequest )
        {
            Log.Info ( "클라이언트 연결이 시작되었습니다" );
            ClientPeer peer = new ClientPeer(initRequest);
            peerList.Add(peer);
            return peer;
        }

        // [tooltip]
        // 초기화 (전체 서버가 시작될 때 호출 됩니다)
        protected override void Setup ( )
        {
            Instance = this;

            // Config : value="%property{Photon:ApplicationLogPath}\\TheLord.Server.log"
            // Path : %YourFolder%\Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\bin_Win64\log
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine ( Path.Combine ( Path.Combine ( this.ApplicationRootPath, "bin_Win64") ), "log" );
            // Path : %YourFolder%\Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\TheLordServer\bin
            FileInfo configFileInfo = new FileInfo ( Path.Combine ( this.BinaryPath, "log4net.config") );

            if(configFileInfo.Exists)
            {
                // Photon Log Plugin 설정
                LogManager.SetLoggerFactory ( Log4NetLoggerFactory.Instance );
                log4net.Config.XmlConfigurator.ConfigureAndWatch ( configFileInfo );
            }

            TheLordTable.Instance.Load ( );
            MongoHelper.ConnectToMongoService ( Log );

            AddHandler ( );

            Log.Info ( "서버 준비 완료!" );
        }

        // [tooltip]
        // 서버가 닫힐 때 호출 됩니다
        protected override void TearDown ( )
        {
            //syncPositionThread.Stop ( );
            RemoveHandler ( );

            Log.Info ( "서버 종료" );
        }


        public void AddHandler ( )
        {
            LoginHandler.Instance.AddListener ( );
            UserAssetHandler.Instance.AddListener ( );
            BuildingHandler.Instance.AddListener ( );
        }

        public void RemoveHandler ( )
        {
            LoginHandler.Instance.RemoveListener ( );
            UserAssetHandler.Instance.RemoveListener ( );
            BuildingHandler.Instance.RemoveListener ( );
        }
    }
}
