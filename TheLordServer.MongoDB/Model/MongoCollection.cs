using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TheLordServer.MongoDB.Model
{
    public abstract class MongoCollection<T>
    {
        protected IMongoDatabase database;
        public IMongoCollection<T> collection { get; set; }

        public MongoCollection ( IMongoDatabase database, string name )
        {
            this.database = database;
            collection = database.GetCollection<T> ( name );
        }

        public async Task<T> Get (ObjectId id)
        {
            try
            {
                var data = await collection.FindAsync(Builders<T>.Filter.Eq("_id", id));
                var datas = data.ToList();
                return datas.Count > 0 ? datas[0] : default ( T );
            }
            catch ( Exception )
            {
                return default ( T );
            }
        }

        public async Task Add(T data)
        {
            await collection.InsertOneAsync ( data );
        }
    }
}