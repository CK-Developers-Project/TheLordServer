using System;
using System.IO;
using System.Collections.Generic;
using ExitGames.Logging;
using ProtoBuf;

namespace TheLordServer.Util
{
    public class BinSerializer
    {
        public static readonly ILogger Log = LogManager.GetCurrentClassLogger ( );

        public static byte[] Serialize<T> ( T t )
        {
            try
            {
                using ( MemoryStream ms = new MemoryStream ( ) )
                {
                    Serializer.Serialize<T> ( ms, t );
                    byte[] result = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read ( result, 0, result.Length );
                    return result;
                }
            }
            catch ( Exception e )
            {
                Log.Info ( "Serialize Fail : " + e.ToString ( ) );
                return null;
            }
        }

        public static T Deserialize<T> ( byte[] msg )
        {
            try
            {
                using ( MemoryStream ms = new MemoryStream ( ) )
                {
                    ms.Write ( msg, 0, msg.Length );
                    ms.Position = 0;
                    return Serializer.Deserialize<T> ( ms );
                }
            }
            catch ( Exception e )
            {
                Log.Info ( "Deserialize Fail : " + e.ToString ( ) );
                return default ( T );
            }
        }

        public static Dictionary<byte, object> ConvertPacket<TValue> ( TValue packet, byte key = 1 )
        {
            return new Dictionary<byte, object> { { key, Serialize ( packet ) } };
        }

        public static T ConvertData<T> ( Dictionary<byte, object> packet, byte key = 1 )
        {
            byte[] bytes = (byte[])DictionaryTool.GetValue ( packet, key );
            return Deserialize<T> ( bytes );
        }
    }
}
