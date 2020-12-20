using MongoDB.Driver;
using System.Threading.Tasks;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class UserAssetCollection : MongoCollection<UserAssetData>
    {
        public UserAssetCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }

        public async Task Update(UserAssetData data)
        {
            try
            {
                //var dbList = await collection.Find ( Builders<UserAssetData>.Filter.Eq ( "_id", data.Id ) ).ToListAsync ( );


                var filter = Builders<UserAssetData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserAssetData>.Update.Set ( ( x ) => x, data );
                await collection.UpdateOneAsync ( filter, update, new UpdateOptions ( ) { IsUpsert = true } );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.Update] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }

        public async Task UpdateResource( UserAssetData data )
        {
            try
            {
                var filter = Builders<UserAssetData>.Filter.Eq ( "_id", data.Id ) &
                             Builders<UserAssetData>.Filter.Eq ( "Resource", data.Resource );
                var update = Builders<UserAssetData>.Update.Set ( ( x ) => x.Resource, data.Resource );
                                                 
                await collection.UpdateOneAsync ( filter, update, new UpdateOptions ( ) { IsUpsert = true } );
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateGold] Error - {1}", typeof ( UserAssetCollection ).Name, e.Message );
            }
        }
    }
}
