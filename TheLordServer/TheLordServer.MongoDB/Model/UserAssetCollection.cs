using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class UserAssetCollection : MongoCollection<UserAssetData>
    {
        public UserAssetCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }

        public async Task UpdateResource( UserAssetData data )
        {
            try
            {
                var filter = Builders<UserAssetData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserAssetData>.Update.Set ( ( x ) => x.Resource, data.Resource );
                                                 
                await collection.UpdateOneAsync ( filter, update );
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateGold] Error - {1}", typeof ( UserAssetCollection ).Name, e.Message );
            }
        }
    }
}
