using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ExitGames.Logging;
using System;
using System.Linq;

namespace TheLordServer.Table
{
    public class BaseTable
    {
        protected static string _splite_return = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        protected static string _line_splite_return = @"\r\n|\n\r|\n|\r";
        protected static char[] _trim_chars = { '\"' };

        protected static string TYPE_INT32 = "int32";
        protected static string TYPE_FLOAT = "float";
        protected static string TYPE_STRING = "string";
        protected static string TYPE_LIST = "List";

        protected static ILogger Log = LogManager.GetCurrentClassLogger ( );

        protected Dictionary<string, List<Dictionary<string, object>>> table;

        public BaseTable(string file)
        {
            table = Read ( file );
        }

        public static Dictionary<string, object> Get(List<Dictionary<string, object>> sheet, string key, int index)
        {
            Dictionary<string, object> result = null;
            foreach (var record in sheet)
            {
                int value = (int)record[key];
                if (value == index)
                {
                    result = record;
                    break;
                }
            }
            return result;
        }


        Dictionary<string, List<Dictionary<string, object>>> Read (string file)
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
                        for (int i = 0; i < lines.Length; ++i)
                        {
                            if (lines[i].Length > 2 && lines[i].Substring(0, 2).Equals("//"))
                            {
                                sheetName = lines[i].Substring(2);
                                header = Regex.Split(lines[i + 1], _splite_return);
                                types = Regex.Split(lines[i + 2], _splite_return);
                                i += 2;
                                table.Add(sheetName, new List<Dictionary<string, object>>());
                                continue;
                            }

                            var values = Regex.Split(lines[i], _splite_return);
                            if (values.Length == 0 || values[0] == "")
                            {
                                continue;
                            }

                            var entry = new Dictionary<string, object>();
                            for (int j = 0; j < header.Length; ++j)
                            {
                                string value = values[j];

                                if (types[j].Contains(TYPE_LIST))
                                {
                                    List<string> list = value.Split('#').ToList();
                                    List<object> temp = new List<object>();
                                    for (int a = 0; a < list.Count; ++a)
                                    {
                                        list[a] = list[a].TrimStart(_trim_chars).TrimEnd(_trim_chars).Replace("\\", "");
                                        list[a] = list[a].Replace("<br>", "\n");
                                        list[a] = list[a].Replace("<c>", ",");
                                        temp.Add(Parsing(types[j], list[a]).Invoke());
                                    }
                                    entry.Add(header[j], temp);
                                }
                                else
                                {
                                    value = value.TrimStart(_trim_chars).TrimEnd(_trim_chars).Replace("\\", "");
                                    value = value.Replace("<br>", "\n");
                                    value = value.Replace("<c>", ",");

                                    entry.Add(header[j], Parsing(types[j], value).Invoke());
                                }
                            }
                            table[sheetName].Add(entry);
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Log.ErrorFormat ( "[BaseTable] - {0}", e.Message );
            }
            return table;
        }

        Func<object> Parsing(string type, string value)
        {
            Func<object> parsing = null;

            if (type.Contains(TYPE_INT32))
            {
                parsing = () => Int32.Parse(value);
            }
            else if (type.Contains(TYPE_FLOAT))
            {
                parsing = () => float.Parse(value);
            }
            else if (type.Contains(TYPE_STRING))
            {
                parsing = () => value;
            }

            return parsing;
        }
    }
}