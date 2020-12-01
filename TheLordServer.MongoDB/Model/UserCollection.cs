using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using System;

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
            try
            {
                var filter = Builders<UserData>.Filter.Eq ( "Username", username );
                collection.DeleteOneAsync ( filter );
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.Remove] Error - {1}", typeof ( UserCollection ).Name, e.Message );
            }
        }

        public void UpdateInfo ( UserData data )
        {
            try
            {
                var filter = Builders<UserData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserData>.Update.Set ( (x)=> x.Info, data.Info );
                collection.UpdateOneAsync ( filter, update );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateInfo] Error - {1}", typeof ( UserCollection ).Name, e.Message );
            }
        }

        public void UpdatePassword ( UserData data )
        {
            try
            {
                var filter = Builders<UserData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserData>.Update.Set ( "Password", data.Password );
                collection.UpdateOneAsync ( filter, update );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdatePassword] Error - {1}", typeof ( UserCollection ).Name, e.Message );
            }
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
            try
            {
                var builder = Builders<UserData>.Filter;
                var filter = builder.Eq ( "Username", username ) & builder.Eq ( "Password", password );
                var data = collection.Find ( filter ).ToListAsync ( ).Result;
                return data.Count > 0 ? true : false;
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.VerifyUser] Error - {1}", typeof ( UserCollection ).Name, e.Message );
                return false;
            }
            
        }
    }
}
