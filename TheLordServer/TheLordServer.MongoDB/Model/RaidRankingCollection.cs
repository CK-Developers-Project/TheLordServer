using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class RaidRankingCollection : MongoCollection<RaidRankingData>
    {
        public RaidRankingCollection(IMongoDatabase database, string name) : base(database, name)
        {
        }

        public async Task Update(RaidRankingData data)
        {
            try
            {
                var dbFilter = Builders<RaidRankingData>.Filter.Eq ( "_id", data.Id );
                var dbList = await collection.Find ( dbFilter ).ToListAsync ( );
                bool bExist = dbList.Count > 0;
                if ( bExist )
                {
                    var filter = Builders<RaidRankingData>.Filter.Eq ( "_id", data.Id );
                    var update = Builders<RaidRankingData>.Update
                                .Set ( ( x ) => x.Key, data.Key )
                                .Set ( ( x ) => x.Score, data.Score )
                                .Set ( ( x ) => x.LastHit, data.LastHit );
                    await collection.UpdateOneAsync ( filter, update, new UpdateOptions { IsUpsert = true } );
                }
                else
                {
                    await Add ( data );
                }
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.Update] Error - {1}", typeof ( RaidRankingData ).Name, e.Message );
            }
        }

        public async Task<List<RaidRankingData>> GetAllSorting()
        {
            try
            {
                var data = await collection.Find ( Builders<RaidRankingData>.Filter.Empty )
                                            .Sort ( Builders<RaidRankingData>.Sort.Descending ( ( x ) => x.Score ) )
                                            .ToListAsync ( );
                return data.Count > 0 ? data : null;
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.GetAllSorting] Error - {1}", typeof ( RaidRankingData ).Name, e.Message );
                return null;
            }
        }
    }
}
