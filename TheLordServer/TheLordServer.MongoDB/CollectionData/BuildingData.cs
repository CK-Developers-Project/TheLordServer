using MongoDB.Bson;

namespace TheLordServer.MongoDB.CollectionData
{
    public class BuildingData : BaseData
    {

        public int Index { get; set; }
        public int LV { get; set; }

        public BuildingData ( ObjectId id ) : base ( id ) { }
    }
}
