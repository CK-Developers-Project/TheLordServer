using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLordServer
{
    using Table;
    using Util;

    public class TheLordTable : Singleton<TheLordTable>
    {
        public BuildingTable BuildingTable { get; private set; }

        public void Load()
        {
            BuildingTable = new BuildingTable( "Table\\BuildingTable.csv" );
        }
    }
}
