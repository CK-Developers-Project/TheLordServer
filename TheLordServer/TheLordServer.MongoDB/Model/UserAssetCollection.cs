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
                var dbFilter = Builders<UserAssetData>.Filter.Eq ( "_id", data.Id );
                var dbList = await collection.Find ( dbFilter ).ToListAsync ( );
                bool bExist = dbList.Count > 0;
                if ( bExist )
                {
                    var filter = Builders<UserAssetData>.Filter.Eq ( "_id", data.Id );
                    var update = Builders<UserAssetData>.Update
                                .Set ( ( x ) => x.Key, data.Key )
                                .Set ( ( x ) => x.Resource, data.Resource );
                    await collection.UpdateOneAsync ( filter, update, new UpdateOptions { IsUpsert = true } );
                }
                else
                {
                    await Add ( data );
                }
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
                var filter = Builders<UserAssetData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserAssetData>.Update.SetOnInsert ( ( x ) => x.Resource, data.Resource );
                                                 
                await collection.UpdateOneAsync ( filter, update );
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateGold] Error - {1}", typeof ( UserAssetCollection ).Name, e.Message );
            }
        }
    }
}
