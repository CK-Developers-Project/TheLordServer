using System;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.CollectionData
{
    public class BossData : BaseData
    {
        public int Index { get; set; }
        public float HP { get; set; }

        public BossData ( ) : base ( )
        {
            Index = 0;
            HP = 0;
        }
    }
}
