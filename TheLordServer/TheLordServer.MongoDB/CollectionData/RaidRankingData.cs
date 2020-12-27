using System;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.CollectionData
{
    public class RaidRankingData : BaseData
    {
        public ObjectId Key { get; set; }
        public string Nickname { get; set; }
        public int Index { get; set; }
        public int Tier { get; set; }
        public int Score { get; set; }
        public bool LastHit { get; set; }

        public RaidRankingData ( ObjectId key ) : base ( )
        {
            Key = key;
            Nickname = "";
            Index = 0;
            Tier = 0;
            Score = 0;
            LastHit = false;
        }
    }
}
