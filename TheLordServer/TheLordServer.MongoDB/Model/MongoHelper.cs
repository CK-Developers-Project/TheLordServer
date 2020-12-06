using System;
using MongoDB.Driver;
using ExitGames.Logging;

namespace TheLordServer.MongoDB.Model
{
    public class MongoHelper
    {
        public static IMongoClient client { get; set; }
        public static IMongoDatabase database { get; set; }
        public static string UserName = "aomgaomgaomg";
        public static string Password = "de132254";
        public static string MongoConnection = "mongodb+srv://{0}:{1}@cluster0.4aeji.gcp.mongodb.net/<dbname>?retryWrites=true&w=majority";
        public static string MongoDatabase = "tsetDB";

        public static UserCollection UserCollection;
        public static UserAssetCollection UserAssetCollection;
        public static BuildingCollection BuildingCollection;

        public static ILogger Log { get; set; }

        public static void ConnectToMongoService( ILogger log ) 
        {
            Log = log;
            try
            {
                string connectionMessage = string.Format(MongoConnection, UserName, Password);
                client = new MongoClient(connectionMessage);
                database = client.GetDatabase(MongoDatabase);

                UserCollection = new UserCollection ( database, "UserData" );
                UserAssetCollection = new UserAssetCollection ( database, "UserAssetData" );
                BuildingCollection = new BuildingCollection ( database, "BuildingData" );
            }
            catch(MongoException e)
            {
                Log.Info ( "MongoDB Login Fail : " + e.Message );
            }
        }
    }
}
