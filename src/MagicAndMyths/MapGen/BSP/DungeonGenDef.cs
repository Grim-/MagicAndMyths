using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenDef : MapGeneratorDef
    {
        public IntRange difficultyRange = new IntRange(1, 5);

        //Dungeon base buildings
        public ThingDef indestructibleWall;
        public ThingDef wall;
        public Thing floor;


        //Dungeon generation setting
        public IntRange randomCorridoorAmount = new IntRange(1, 2);
        public int maxDepth = 8;
        public int minRoomSize = 8;
        public int minRoomPadding = 3;

        //larger factor more of its BSP partition it takes
        public float roomSizeFactor = 0.65f;

        public IntRange roomAmount = new IntRange(4, 5);
        public float minSizeMultiplier = 1.2f;
        public float aspectRatioThreshold = 1.3f;
        public float edgeMarginDivisor = 1.5f;
        public IntRange sideRoomCount = new IntRange(3, 6);
        public bool allowHiddenSidePaths = true;
        public float hiddenSidePathChance = 0.3f;
        public bool addRandomCorridoors = true;

        public List<RoomTypeDef> availableRoomTypes;
        public List<CelluarAutomataDef> earlyAutomata;
        public List<CelluarAutomataDef> postGenAutomata;
    }
}
