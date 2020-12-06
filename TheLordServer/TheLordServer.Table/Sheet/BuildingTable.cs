using System.Collections.Generic;

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
