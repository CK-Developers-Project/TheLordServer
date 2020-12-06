using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TheLordServer.Table
{
    public class BuildingTable : BaseTable
    {
        public List<Dictionary<string, object>> BuildingInfoSheet { get; private set; }
        public List<Dictionary<string, object>> MainBuildingInfoSheet { get; private set; }

        public BuildingTable ( string file ) : base(file)
        {
            BuildingInfoSheet = table["BuildingInfo"];
            MainBuildingInfoSheet = table["MainBuildingInfo"];
        }


    }
}
