using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TheLordServer.MongoDB.CollectionData
{
    public class UserAssetData : BaseData
    {
        public class UserResource
        {
            public string Gold;
            public string Cash;
        }

        public UserResource Resource;

        public UserAssetData ( ref ObjectId id ) : base ( ref id ) { }
    }
}
