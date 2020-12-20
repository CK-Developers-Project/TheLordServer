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
                var dbFilter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                var dbList = await collection.Find ( dbFilter ).ToListAsync ( );
                bool bExist = dbList.Count > 0;
                if ( bExist )
                {
                    var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                    var update = Builders<BuildingData>.Update
                                .SetOnInsert ( ( x ) => x.Index, data.Index )
                                .SetOnInsert ( ( x ) => x.LV, data.LV )
                                .SetOnInsert ( ( x ) => x.WorkTime, data.WorkTime )
                                .SetOnInsert ( ( x ) => x.CharactertData, data.CharactertData );
                    await collection.UpdateOneAsync ( filter, update );
                }
                else
                {
                    await Add ( data );
                }
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
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<BuildingData>.Update.SetOnInsert ( ( x ) => x.LV, data.LV );
                await collection.UpdateOneAsync ( filter, update );
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
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<BuildingData>.Update.SetOnInsert ( ( x ) => x.WorkTime, data.WorkTime );

                await collection.UpdateOneAsync ( filter, update );
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
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<BuildingData>.Update.SetOnInsert ( ( x ) => x.CharactertData, data.CharactertData );

                await collection.UpdateOneAsync ( filter, update );
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateCharactertData] Error - {1}", typeof ( BuildingData ).Name, e.Message );
            }
        }

        public async Task<BuildingData> GetByIndex (ObjectId key, int index)
        {
            try
            {
                var filter = Builders<BuildingData>.Filter.Eq ( "Key", key ) &
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
