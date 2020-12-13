using System;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.CollectionData
{
    public class BuildingData : BaseData
    {

        public int Index { get; set; }
        public int LV { get; set; }
        public DateTime WorkTime;
        public BuildingData ( ObjectId id ) : base ( id ) { }
    }
}
