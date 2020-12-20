using System;
using MongoDB.Bson;

namespace TheLordServer.MongoDB.CollectionData
{
    public class BuildingData : BaseData
    {
        public class CharacterData
        {
            public int Index { get; set; }
            public int Amount { get; set; }
            public CharacterData()
            {
                Amount = 0;
            }
        }

        public ObjectId Key { get; set; }
        public int Index { get; set; }
        public int LV { get; set; } 
        public DateTime WorkTime { get; set; } 
        public CharacterData CharactertData { get; set; } 

        public BuildingData ( ObjectId key ) : base ( ) 
        {
            Key = key;
            Index = 0;
            LV = 0;
            WorkTime = default;
            CharactertData = new CharacterData ( );
        }
    }
}
