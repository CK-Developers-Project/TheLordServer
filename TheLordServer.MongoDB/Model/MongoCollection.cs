using System;
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
    }
}