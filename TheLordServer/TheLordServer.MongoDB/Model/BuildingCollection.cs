﻿using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using System;

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
                var filter = Builders<BuildingData>.Filter.Eq ( "_id", data.Id ) &
                             Builders<BuildingData>.Filter.Eq( "Index", data.Index);
                var update = Builders<BuildingData>.Update.Set ( ( x ) => x.LV, data.LV );
                await collection.UpdateOneAsync ( filter, update );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateLV] Error - {1}", typeof ( BuildingData ).Name, e.Message );
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
