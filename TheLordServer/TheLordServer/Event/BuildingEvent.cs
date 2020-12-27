using Photon.SocketServer;

namespace TheLordServer.Event
{
    using Table.Structure;
    using Util;

    public class BuildingEvent
    {
        public static void OnUpdateBuilding(ClientPeer peer, int index)
        {
            TheLordServer.Log.InfoFormat ( "[OnUpdateBuilding] - {0}", peer.LocalIP );
            if (peer.userAgent.UserData == null)
            {
                // 로그인 씬으로
                return;
            }

            EventData data = new EventData ( (byte)EventCode.UpdateBuilding );
            var buildingData = peer.userAgent.BuildingDataList.Find ( x => x.Index == index );
            
            if(buildingData == null)
            {
                // 건물 없음
                return;
            }

            var packet = new ProtoData.BuildingData ( );
            packet.index = index;
            packet.LV = buildingData.LV;
            packet.tick = buildingData.WorkTime.Equals(default) ? -1 : GameUtility.RemaineTick ( buildingData.WorkTime );
            packet.amount = buildingData.CharactertData.Amount;
            data.Parameters = BinSerializer.ConvertPacket ( packet );
            peer.SendEvent ( data, new SendParameters ( ) );
        }
    }
}
