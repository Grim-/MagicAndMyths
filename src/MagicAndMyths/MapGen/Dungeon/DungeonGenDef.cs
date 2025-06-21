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

        public ThingDef largeDoor;
        public TerrainDef floor;

        public ThingDef IndestructibleWallDef => indestructibleWall != null ? indestructibleWall : MagicAndMythDefOf.DungeonWall;
        public ThingDef WallDef => wall != null ? wall : ThingDefOf.Wall;
        public ThingDef WallStuffDef => wallStuff != null ? wallStuff : GenStuff.DefaultStuffFor(WallDef);


        public ThingDef DoorDef => largeDoor != null ? largeDoor : ThingDefOf.SecurityDoor;

        public TerrainDef TerrainDef => floor != null ? floor : TerrainDefOf.MetalTile;


        //Dungeon generation setting
        public IntRange randomCorridoorAmount = new IntRange(1, 2);
        public int maxDepth = 8;
        public IntRange roomSize = new IntRange(8, 8);
        public int minRoomPadding = 2;

        public IntVec3 mapSize = new IntVec3(80, 1, 80);
        public FloatRange roomSizeFactor = new FloatRange(0.9f, 1f);
        public IntRange roomAmount = new IntRange(4, 5);


        public IntRange sideRoomCount = new IntRange(3, 6);
        public bool allowHiddenSidePaths = true;
        public float hiddenSidePathChance = 0.3f;
        public bool addRandomCorridoors = true;

        public IntRange sidePathLength = new IntRange(1, 3);
        public float longSidePathChance = 0.4f;
        public float meanderingChance = 0.3f;
        public bool allowBranchingSidePaths = true;
        public float branchingChance = 0.7f;
        public int maxSidePathBranches = 4;

        public List<RoomLayoutData> availableRoomTypes;
        public List<RoomLayoutData> availableSideRoomTypes;


        public List<CelluarAutomataSteps> earlyAutomata;
        public List<CelluarAutomataSteps> postGenAutomata;

        public RoomLayoutData GetRoomTypeDef(DungeonGenerationContext generationContext, DungeonRoom DungeonRoom)
        {
            List<RoomLayoutData> rooms = availableRoomTypes.Where(x => x.def.roomType == RoomType.Normal && x.def.CanApply(generationContext, DungeonRoom)).ToList();
            return rooms.RandomElement();
        }

        public RoomLayoutData GetSideRoomTypeDef(DungeonGenerationContext generationContext, DungeonRoom DungeonRoom)
        {
            List<RoomLayoutData> rooms = availableSideRoomTypes.Where(x => x.def.roomType == RoomType.Normal && x.def.CanApply(generationContext, DungeonRoom)).ToList();
            return rooms.RandomElement();
        }
    }
}
