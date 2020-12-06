using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class BuildingCollection : MongoCollection<BuildingData>
    {
        public BuildingCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }

        public async Task UpdateLV(BuildingData data)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<BuildingData>.Update.Set ( ( x ) => x.LV, data.LV );
                await collection.UpdateOneAsync ( filter, update );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateLV] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }
    }
}
