using System.Collections.Generic;

namespace TheLordServer.Table
{
    public class AbilityTable : BaseTable
    {
        public List<Dictionary<string, object>> AbilityInfoSheet { get; private set; }

        public AbilityTable ( string file ) : base ( file )
        {
            AbilityInfoSheet = table["AbilityInfo"];
        }
    }
}