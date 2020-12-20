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

        public async Task Update(BuildingData data)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<BuildingData>.Update.Set ( ( x ) => x, data );
                await collection.UpdateOneAsync ( filter, update, new UpdateOptions ( ) { IsUpsert = true } );
            }
            catch (MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.Update] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }

        public async Task UpdateLV(BuildingData data)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id ) &
                             Builders<BuildingData>.Filter.Eq( "Index", data.Index);
                var update = Builders<BuildingData>.Update.Set ( ( x ) => x.LV, data.LV );
                await collection.UpdateOneAsync ( filter, update, new UpdateOptions ( ) { IsUpsert = true } );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateLV] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }

        public async Task UpdateWorkTime(BuildingData data)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id ) &
                             Builders<BuildingData>.Filter.Eq ( "WorkTime", data.WorkTime );
                var update = Builders<BuildingData>.Update.Set ( ( x ) => x.WorkTime, data.WorkTime );

                await collection.UpdateOneAsync ( filter, update, new UpdateOptions ( ) { IsUpsert = true } );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateWorkTime] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }

        public async Task UpdateCharactertData(BuildingData data)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id ) &
                             Builders<BuildingData>.Filter.Eq ( "CharactertData", data.CharactertData );
                var update = Builders<BuildingData>.Update.Set ( ( x ) => x.CharactertData, data.CharactertData );

                await collection.UpdateOneAsync ( filter, update, new UpdateOptions ( ) { IsUpsert = true } );
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateCharactertData] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }

        public async Task<BuildingData> GetByIndex (ObjectId id, int index)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", id ) &
                             Builders<BuildingData>.Filter.Eq ( "Index", index );
                var data = await collection.FindAsync ( filter );
                var datas = data.ToList ( );
                return datas.Count > 0 ? datas[0] : null;
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.GetByIndex] Error - {1}", typeof ( UserCollection ).Name, e.Message );
                return null;
            }
        }
    }
}
