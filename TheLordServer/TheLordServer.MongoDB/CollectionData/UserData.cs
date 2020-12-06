using System;

namespace TheLordServer.MongoDB.CollectionData
{
    public class UserData : BaseData
    {
        public class UserInfo
        {
            public string Nickname { get; set; }
            public int Race { get; set; }
            public UserInfo ( )
            {
                Nickname = "";
                Race = 0;
            }
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
