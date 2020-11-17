using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TheLordServer.MongoDB.Structure
{
    public abstract class BaseData
    {
        [BsonId]
        public Object Id { get; set; }

        public BaseData ( )
        {
            Id = ObjectId.GenerateNewId ( );
        }
    }

    public class UserData : BaseData
    {
        public class UserInfo
        {
            public string nickname = "";
            public int race = 0;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime RegisterTime { get; set; }

        public UserInfo Info { get; set; }

        public UserData ( string username, string password, DateTime dateTime ) : base ( )
        {
            Username = username;
            Password = password;
            RegisterTime = dateTime;
            Info = new UserInfo ( );
        }
    }


}
