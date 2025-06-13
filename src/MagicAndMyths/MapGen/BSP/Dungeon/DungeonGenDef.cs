using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenDef : MapGeneratorDef
    {
        public IntRange difficultyRange = new IntRange(1, 5);

        public float noRoomChanceCriticalPath = 0.2f;
        public float noRoomChanceSidePath = 0.5f;

        //Dungeon base buildings
        public ThingDef indestructibleWall;
        public ThingDef wall;
        public ThingDef wallStuff;

        public TerrainDef floor;

        public ThingDef IndestructibleWallDef => indestructibleWall != null ? indestructibleWall : MagicAndMythDefOf.DungeonWall;
        public ThingDef WallDef => wall != null ? wall : ThingDefOf.Wall;
        public ThingDef WallStuffDef => wallStuff != null ? wallStuff : GenStuff.DefaultStuffFor(WallDef);

        public TerrainDef TerrainDef => floor != null ? floor : TerrainDefOf.MetalTile;


        //Dungeon generation setting
        public IntRange randomCorridoorAmount = new IntRange(1, 2);
        public int maxDepth = 8;
        public int minRoomSize = 8;
        public int minRoomPadding = 2;

        public IntVec3 mapSize = new IntVec3(80, 1, 80);

        //larger factor more of its BSP partition it takes
        public FloatRange roomSizeFactor = new FloatRange(0.9f, 1f);

        public IntRange roomAmount = new IntRange(4, 5);
        public float minSizeMultiplier = 1.2f;
        public float aspectRatioThreshold = 1.3f;
        public float edgeMarginDivisor = 1.5f;
        public IntRange sideRoomCount = new IntRange(3, 6);
        public bool allowHiddenSidePaths = true;
        public float hiddenSidePathChance = 0.3f;
        public bool addRandomCorridoors = true;


        public IntRange sidePathLength = new IntRange(1, 8);
        public float longSidePathChance = 0.4f;
        public float meanderingChance = 0.3f;
        public bool allowBranchingSidePaths = true;
        public float branchingChance = 0.5f;
        public int maxSidePathBranches = 4;

        public List<RoomTypeDef> availableRoomTypes;
        public List<RoomTypeDef> availableSideRoomTypes;


        public List<CelluarAutomataSteps> earlyAutomata;
        public List<CelluarAutomataSteps> postGenAutomata;



        public RoomTypeDef GetRoomTypeDef(Dungeon Dungeon, DungeonRoom DungeonRoom)
        {
            List<RoomTypeDef> rooms = availableRoomTypes.Where(x => x.roomType == RoomType.Normal).ToList();
            return rooms.RandomElement();
        }

        public RoomTypeDef GetSideRoomTypeDef(Dungeon Dungeon, DungeonRoom DungeonRoom)
        {
            List<RoomTypeDef> rooms = availableSideRoomTypes.Where(x => x.roomType == RoomType.Normal).ToList();
            return rooms.RandomElement();
        }
    }


    public class CelluarAutomataSteps
    {
        public CelluarAutomataDef automataDef;
        public int iterations = 1;
        public int order = 100;
    }
}
