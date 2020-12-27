using System.Collections.Generic;
using System;
using Photon.SocketServer;
using ExitGames.Logging;

namespace TheLordServer
{
    using Table.Structure;

    public class HandlerMedia
    {
        public delegate void Act ( ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters );
        private static Dictionary<OperationCode, Delegate> msgTable = new Dictionary<OperationCode, Delegate> ( );
        public static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static void AddListener ( OperationCode type, Act act )
        {
            if ( !msgTable.ContainsKey ( type ) )
            {
                msgTable.Add ( type, null );
            }

            Delegate dgt = msgTable[type];
            if ( dgt != null && dgt.GetType ( ) != act.GetType ( ) )
            {
                Log.ErrorFormat (
                    "Event Type {0}에 대한 서명이 일치하지 않은 Listener를 추가하려했습니다.\n" +
                    "현재 Listener에는 {1} Type이 있으며 추가 요청중인 Listener {2} Type이 있습니다."
                    , type, dgt.GetType ( ).Name, act.GetType ( ).Name
                );
            }
            else
            {
                msgTable[type] = (Act)msgTable[type] + act;
            }
        }

        public static void RemoveListener ( OperationCode type, Act act )
        {
            if ( msgTable.ContainsKey ( type ) )
            {
                Delegate dgt = msgTable[type];

                if ( dgt == null )
                {
                    Log.ErrorFormat (
                        "Event Type {0}에 대한 Listener 제거를 시도했지만 현재 Listener가 null입니다.", type );
                }
                else if ( dgt.GetType ( ) != act.GetType ( ) )
                {
                    Log.ErrorFormat (
                        "Event Type {0}에 대한 서명이 일치하지 않는 Listener를 제거하려고 합니다." +
                        "현재 Listener에는 {1} Type가 있으며 제가하려는 Listener에는 {2} Type이 있습니다.",
                        type, dgt.GetType ( ).Name, act.GetType ( ).Name );
                }
                else
                {
                    msgTable[type] = (Act)msgTable[type] - act;
                    if ( dgt == null )
                    {
                        msgTable.Remove ( type );
                    }
                }
            }
            else
            {
                Log.ErrorFormat ( "{0} Type에 대한 Listener를 제거하려고 시도했지만 Messenger에 해당 Event Type이 없습니다.", type );
            }
        }

        public static void RemoveAllListener ( OperationCode type )
        {
            if ( msgTable.ContainsKey ( type ) )
            {
                msgTable[type] = null;
                msgTable.Remove ( type );
            }
            else
            {
                Log.ErrorFormat ( "{0} Type에 대한 Listener를 제거하려 시도했지만 Messenger에 해당 Event Type이 없습니다.", type );
            }
        }

        public static void Dispatch ( OperationCode type, ClientPeer peer, OperationRequest operationRequest, SendParameters sendParameters )
        {
            TheLordServer.Log.InfoFormat ( "[{0}]", type );
            Delegate dgt;
            if ( msgTable.TryGetValue ( type, out dgt ) )
            {
                Act callback = dgt as Act;
                if ( callback != null )
                {
                    callback ( peer, operationRequest, sendParameters );
                }
                else
                {
                    Log.ErrorFormat ( "Event Type {0}를 찾기 못했습니다.", type );
                }
            }
        }
    }
}
