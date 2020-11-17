using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.Model
{
    using Structure;

    public class UserCollection : MongoCollection<UserData>
    {
        public UserCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }

        public void Add ( UserData data )
        {
            collection.InsertOneAsync ( data );
        }

        public void Remove ( string username )
        {
            var filter = Builders<UserData>.Filter.Eq ( "Username", username );
            collection.DeleteOneAsync ( filter );
        }

        public void UpdatePassword ( string username, string password )
        {
            var filter = Builders<UserData>.Filter.Eq ( "Username", username );
            var update = Builders<UserData>.Update.Set ( "Password", password );
            collection.UpdateOneAsync ( filter, update );
        }

        public UserData GetByUsername ( string username )
        {
            var data = collection.Find ( Builders<UserData>.Filter.Eq ( "Username", username ) ).ToList();
            return data.Count > 0 ? data[0] : null;
        }

        public List<UserData> GetAllUsers ( )
        {
            return collection.Find ( Builders<UserData>.Filter.Empty ).ToListAsync ( ).Result;
        }

        public bool VerifyUser ( string username, string password )
        {
            var builder = Builders<UserData>.Filter;
            var filter = builder.Eq ( "Username", username ) & builder.Eq ( "Password", password );
            var data = collection.Find ( filter ).ToListAsync ( ).Result;
            return data.Count > 0 ? true : false;
        }
    }
}
