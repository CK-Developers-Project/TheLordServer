namespace TheLordServer
{
    using Table;
    using Util;

    public class TheLordTable : Singleton<TheLordTable>
    {
        public AbilityTable AbilityTable { get; private set; }
        public CharacterTable CharacterTable { get; private set; }
        public BuildingTable BuildingTable { get; private set; }

        public void Load()
        {
            AbilityTable = new AbilityTable ( "Table\\AbilityTable.csv" );
            CharacterTable = new CharacterTable ( "Table\\CharacterTable.csv" );
            BuildingTable = new BuildingTable( "Table\\BuildingTable.csv" );
        }
    }
}
