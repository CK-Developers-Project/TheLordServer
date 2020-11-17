using System;
using MongoDB.Driver;

namespace TheLordServer.MongoDB.Model
{
    using Structure;
    public class MongoHelper
    {
        public static IMongoClient client { get; set; }
        public static IMongoDatabase database { get; set; }
        public static string UserName = "aomgaomgaomg";
        public static string Password = "de132254";
        public static string MongoConnection = "mongodb+srv://{0}:{1}@cluster0.4aeji.gcp.mongodb.net/<dbname>?retryWrites=true&w=majority";
        public static string MongoDatabase = "tsetDB";

        public static UserCollection UserCollection;

        public static void ConnectToMongoService( ExitGames.Logging.ILogger log ) 
        {
            try
            {
                string connectionMessage = string.Format(MongoConnection, UserName, Password);
                client = new MongoClient(connectionMessage);
                database = client.GetDatabase(MongoDatabase);

                UserCollection = new UserCollection ( database, "UserData" );

            }
            catch(Exception e)
            {
                log.Info ( "MongoDB Login Fail : " + e.Message );
            }
        }
    }
}
