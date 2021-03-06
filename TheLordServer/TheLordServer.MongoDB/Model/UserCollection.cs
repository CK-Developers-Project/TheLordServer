﻿using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class UserCollection : MongoCollection<UserData>
    {
        public UserCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }

        public async Task UpdateInfo ( UserData data )
        {
            try
            {
                var filter = Builders<UserData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserData>.Update.Set ( ( x ) => x.Info, data.Info );
                await collection.UpdateOneAsync ( filter, update );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdateInfo] Error - {1}", typeof ( UserCollection ).Name, e.Message );
            }
        }

        public async Task UpdatePassword ( UserData data )
        {
            try
            {
                var filter = Builders<UserData>.Filter.Eq ( "_id", data.Id );
                var update = Builders<UserData>.Update.Set ( "Password", data.Password );
                await collection.UpdateOneAsync ( filter, update );
            }
            catch ( MongoException e )
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.UpdatePassword] Error - {1}", typeof ( UserCollection ).Name, e.Message );
            }
        }

        public async Task<UserData> GetByUsername ( string username )
        {
            try
            {
                var data = await collection.FindAsync ( Builders<UserData>.Filter.Eq ( "Username", username ) );
                var datas = data.ToList ( );
                return datas.Count > 0 ? datas[0] : null;
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.GetByUsername] Error - {1}", typeof ( UserCollection ).Name, e.Message );
                return null;
            }
        }

        public async Task<List<UserData>> GetAllUsers ( )
        {
            try
            {
                var data = await collection.Find ( Builders<UserData>.Filter.Empty ).ToListAsync ( );
                return data;
            }
            catch(MongoException e)
            {
                MongoHelper.Log.ErrorFormat ( "[{0}.GetAllUsers] Error - {1}", typeof ( UserCollection ).Name, e.Message );
                return null;
            }
        }

        public async Task<bool> VerifyUser ( string username, string password )
        {
            try
            {
                var builder = Builders<UserData>.Filter;
                var filter = builder.Eq ( "Username", username ) & builder.Eq ( "Password", password );
                var data = await collection.Find ( filter ).ToListAsync ( );
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
