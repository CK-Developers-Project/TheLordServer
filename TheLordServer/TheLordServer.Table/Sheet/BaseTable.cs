using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ExitGames.Logging;
using System;
using System.Linq;

namespace TheLordServer.Table
{
    public abstract class BaseTable
    {
        protected static string _splite_return = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        protected static string _line_splite_return = @"\r\n|\n\r|\n|\r";
        protected static char[] _trim_chars = { '\"' };

        protected static ILogger Log = LogManager.GetCurrentClassLogger ( );

        protected Dictionary<string, List<Dictionary<string, object>>> table;

        public BaseTable(string file)
        {
            table = Read ( file );
        }

        protected Dictionary<string, List<Dictionary<string, object>>> Read (string file)
        {
            Dictionary<string, List<Dictionary<string, object>>> table = new Dictionary<string, List<Dictionary<string, object>>> ( );

            try
            {
                using ( FileStream fs = new FileStream ( file, FileMode.Open ) )
                {
                    using ( StreamReader sr = new StreamReader ( fs, Encoding.UTF8, false ) )
                    {
                        var lines = Regex.Split ( sr.ReadToEnd ( ), _line_splite_return );
                        string sheetName = "";
                        string[] header = null;
                        string[] types = null;
                        for ( int i = 0; i < lines.Length; ++i )
                        {
                            if ( lines[i].Length > 2 && lines[i].Substring ( 0, 2 ).Equals ( "//" ) )
                            {
                                sheetName = lines[i].Substring ( 2 );
                                header = Regex.Split ( lines[i + 1], _splite_return );
                                types = Regex.Split ( lines[i + 2], _splite_return );
                                i += 2;
                                table.Add ( sheetName, new List<Dictionary<string, object>> ( ) );
                                continue;
                            }

                            var values = Regex.Split ( lines[i], _splite_return );
                            if ( values.Length == 0 || values[0] == "" )
                            {
                                continue;
                            }
                            
                            var entry = new Dictionary<string, object> ( );
                            for ( int j = 0; j < header.Length; ++j )
                            {
                                string value = values[j];
                                
                                if ( types[j].Contains ( "List" ) )
                                {
                                    List<string> list = value.Split ( '#' ).ToList ( );
                                    for ( int a = 0; a < list.Count; ++a )
                                    {
                                        string temp = list[a];
                                        temp = temp.TrimStart ( _trim_chars ).TrimEnd ( _trim_chars ).Replace ( "\\", "" );
                                        list[a] = temp;
                                    }
                                    entry.Add ( header[j], list );
                                }
                                else
                                {
                                    value = value.TrimStart ( _trim_chars ).TrimEnd ( _trim_chars ).Replace ( "\\", "" );
                                    value = value.Replace ( "<br>", "\n" );
                                    value = value.Replace ( "<c>", "," );

                                    if(types[j].Contains("int32"))
                                    {
                                        entry.Add ( header[j], Int32.Parse(value) );
                                    }
                                    else if(types[j].Contains("float"))
                                    {
                                        entry.Add ( header[j], float.Parse(value) );
                                    }
                                    else
                                    {
                                        entry.Add ( header[j], value );
                                    }
                                }
                            }
                            table[sheetName].Add ( entry );
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Log.InfoFormat ( "[BaseTable] - {0}", e.Message );
            }
            return table;
        }
        
        public static Dictionary<string, object> Get(List<Dictionary<string, object>> sheet, string key, int index)
        {
            Dictionary<string, object> result = null;
            foreach(var record in sheet)
            {
                int value = (int)record[key];
                if ( value == index )
                {
                    result = record;
                    break;
                }
            }
            return result;
        }
    }
}