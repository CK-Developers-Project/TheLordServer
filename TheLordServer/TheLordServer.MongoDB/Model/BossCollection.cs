using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class BossCollection : MongoCollection<BossData>
    {
        public BossCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }

        public async Task Update ( BossData data )
        {
            try
            {
                var dbFilter = Builders<BossData>.Filter.Eq ( "_id", data.Id );
                var dbList = await collection.Find ( dbFilter ).ToListAsync ( );
                bool bExist = dbList.Count > 0;
                if ( bExist )
                {
                    var filter = Builders<BossData>.Filter.Eq ( "_id", data.Id );
                    var update = Builders<BossData>.Update
                                .Set ( ( x ) => x.Index, data.Index )
                                .Set ( ( x ) => x.HP, data.HP );
                    await collection.UpdateOneAsync ( filter, update, new UpdateOptions { IsUpsert = true } );
                }
                else
                {
                    await Add ( data );
                }
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.Update] Error - {1}", typeof ( BossData ).Name, e.Message );
            }
        }

        public async Task<BossData> GetByIndex(int index)
        {
            try
            {
                var data = await collection.FindAsync ( Builders<BossData>.Filter.Eq ( "Index", index ) );
                var datas = data.ToList ( );
                return datas.Count > 0 ? datas[0] : null;
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.GetByIndex] Error - {1}", typeof ( BossData ).Name, e.Message );
                return null;
            }
        }
    }
}
