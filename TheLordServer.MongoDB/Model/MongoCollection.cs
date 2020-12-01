using System;
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

        public T Get (ObjectId id)
        {
            try
            {
                var data = collection.Find ( Builders<T>.Filter.Eq ( "_id", id ) ).ToList ( );
                return data.Count > 0 ? data[0] : default ( T );
            }
            catch ( Exception )
            {
                return default ( T );
            }
        }
    }
}