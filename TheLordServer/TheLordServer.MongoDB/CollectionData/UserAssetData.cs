using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TheLordServer.MongoDB.CollectionData
{
    using Util;

    public class UserAssetData : BaseData
    {
        public class UserResource
        {
            public string Gold { get; set; }
            public string Cash { get; set; }

            public UserResource()
            {
                Gold = "0";
                Cash = "0";
            }
        }

        public UserResource Resource { get; set; }

        public UserAssetData ( ObjectId id ) : base ( id ) 
        {
            Resource = new UserResource ( );
        }

        [BsonIgnore]
        public string Gold {
            get => Resource.Gold;
            set => Resource.Gold = value;
        }

        [BsonIgnore]
        public string Cash {
            get => Resource.Cash;
            set => Resource.Cash = value;
        }

        public void AddGold(BigInteger value)
        {
            BigInteger gold = new BigInteger ( Gold );
            gold += value;
            Gold = gold.ToString ( );
        }

        public BigInteger GetGold()
        {
            return new BigInteger ( Gold );
        }

        public void AddCash(BigInteger value)
        {
            BigInteger cash = new BigInteger ( Cash );
            cash += value;
            Cash = cash.ToString ( );
        }

        public BigInteger GetCash()
        {
            return new BigInteger ( Cash );
        }
    }
}
