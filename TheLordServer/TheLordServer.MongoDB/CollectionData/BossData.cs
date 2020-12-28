using System;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.CollectionData
{
    public class BossData : BaseData
    {
        public int Index { get; set; }
        public int HP { get; set; }
        public DateTime CreateTime { get; set; }

        public BossData ( ) : base ( )
        {
            Index = 0;
            HP = 0;
            CreateTime = default;
        }
    }
}
