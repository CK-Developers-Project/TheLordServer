using System;
using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Table.Structure;
    using Table;
    using Util;

    public class BuildingEvent
    {
        public static void OnUpdateBuildTime(ClientPeer peer, int index)
        {
            if(peer.userAgent.UserData == null)
            {
                // 로그인 씬으로
                return;
            }

            EventData data = new EventData ( (byte)EventCode.UpdateBuilding );
            var buildingData = peer.userAgent.BuildingDataList.Find ( x => x.Index == index );
            
            var packet = new ProtoData.BuildingData ( );
            packet.index = index;
            packet.LV = buildingData.LV;
            packet.tick = buildingData.WorkTime.ToString();
            packet.amount = buildingData.CharactertData.Amount;
            data.Parameters = BinSerializer.ConvertPacket ( packet );
            peer.SendEvent ( data, new SendParameters ( ) );
        }
    }
}
