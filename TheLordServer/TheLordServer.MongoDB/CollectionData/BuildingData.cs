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

        public int Index { get; set; }
        public int LV { get; set; } 
        public DateTime WorkTime { get; set; } 
        public CharacterData CharactertData { get; set; } 

        public BuildingData ( ObjectId id ) : base ( id ) 
        {
            CharactertData = new CharacterData ( );
        }
    }
}
