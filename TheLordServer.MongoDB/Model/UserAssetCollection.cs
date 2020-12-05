using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using System;

namespace TheLordServer.MongoDB.Model
{
    using CollectionData;

    public class UserAssetCollection : MongoCollection<UserAssetData>
    {
        public UserAssetCollection ( IMongoDatabase database, string name ) : base ( database, name )
        {
        }


    }
}
