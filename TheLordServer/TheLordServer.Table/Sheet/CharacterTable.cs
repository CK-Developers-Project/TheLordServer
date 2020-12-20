using System.Collections.Generic;

namespace TheLordServer.Table
{
    public class CharacterTable : BaseTable
    {
        public List<Dictionary<string, object>> CharacterInfoSheet { get; private set; }

        public CharacterTable ( string file ) : base ( file )
        {
            CharacterInfoSheet = table["CharacterInfo"];
        }
    }
}