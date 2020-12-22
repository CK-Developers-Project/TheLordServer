using System;
using System.Collections.Generic;

namespace TheLordServer.Util
{
    public static class GameUtility
    {
        static string[] ordinals = new[] { "", "K", "M", "T", "q", "Q", "s", "S", "O", "N", "D" };

        public static string Ordinal ( BigInteger num )
        {
            BigInteger temp = num;
            BigInteger origin = temp;
            int space = 0;

            while ( temp >= 1000 )
            {
                temp /= 1000;
                ++space;
            }

            if ( space >= ordinals.Length )
            {
                space = ordinals.Length - 1;
            }

            string strOrigin = origin.ToString ( ), strTemp = temp.ToString ( );

            int d = 1;
            string c = "0";

            if ( space > 0 )
            {
                d = strOrigin.Length - strTemp.Length;
                c = strOrigin.Substring ( strTemp.Length, ( d < 2 ) ? d : 2 );
            }

            return string.Format ( "{0} {1}", strTemp + ( ( d > 0 && int.Parse ( c ) == 0 ) ? "" : "," + c ), ordinals[space] );
        }


        public static DateTime Now()
        {
            return DateTime.SpecifyKind ( DateTime.UtcNow, DateTimeKind.Utc );
        }

        public static long RemaineTick ( TimeSpan timespan )
        {
            return Math.Max ( 0, timespan.Ticks );
        }

        public static long RemaineTick ( DateTime source )
        {
            return Math.Max ( 0, ( source - Now ( ) ).Ticks );
        }

        public static string DateTime2String(DateTime dateTime)
        {
            if ( dateTime.Equals ( default ) )
            {
                return "0";
            }
            else
            {
                return dateTime.ToString ( "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK" );
            }
        }

        public static DateTime String2DateTime ( string str )
        {
            DateTime dt = default;
            if ( !IsStringNull ( str ) )
            {
                dt = DateTimeOffset.Parse ( str ).DateTime;
            }
            return dt;
        }

        public static bool IsStringNull ( string str )
        {
            return str.Equals ( "0" );
        }
    }
}
