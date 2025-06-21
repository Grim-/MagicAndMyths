using Verse;

namespace MagicAndMyths
{
    public class DungeonGenerationContext
    {
        public Dungeon Dungeon { get; }
        public DungeonGenDef Def { get; }
        public Map Map { get; }
        public int MapMargin { get; set; } = 4;

        public int CorridoorWidth { get; set; } = 3;

        public int MainPathLength { get; set; } = 3;
        public IntVec3 MapSize { get; set; }


        public DoorPlacementManager DoorManager { get; set; }

        public DungeonConstructionService Constructor { get; private set; }
   
        public DungeonGenerationContext(Dungeon dungeon, DungeonGenDef def, Map map)
        {
            Dungeon = dungeon;
            Def = def;
            MainPathLength = Def.roomAmount.RandomInRange;
            MapSize = Def.mapSize;
            Map = map;
            DoorManager = new DoorPlacementManager(this);
            Constructor = new DungeonConstructionService(this);
        }
    }
}
