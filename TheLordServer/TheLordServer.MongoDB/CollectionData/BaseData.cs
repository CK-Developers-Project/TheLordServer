using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TheLordServer.MongoDB.CollectionData
{
    public abstract class BaseData
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public BaseData ( )
        {
            Id = ObjectId.GenerateNewId ( );
        }

        public BaseData ( ObjectId id )
        {
            Id = id;
        }
    }
}
