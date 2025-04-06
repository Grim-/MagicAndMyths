using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class GenStepDef_BspDungeon : GenStepDef
    {
        public IntRange randomCorridoorAmount = new IntRange(1,2);
        public int maxDepth = 8;
        public int minRoomSize = 8;
        public int minRoomPadding = 3;

        //larger factor more of its BSP partition it takes
        public float roomSizeFactor = 0.65f;

        public int minRooms = 5;
        public int maxRooms = 8;
        public float minSizeMultiplier = 1.2f;
        public float aspectRatioThreshold = 1.3f;
        public float edgeMarginDivisor = 1.5f;

        public bool addRandomCorridoors = true;

        public List<RoomTypeDef> availableRoomTypes;
        public List<CelluarAutomataDef> earlyAutomata;
        public List<CelluarAutomataDef> postGenAutomata;
    }

    public class GenStep_BspDungeon : GenStep
    {
        GenStepDef_BspDungeon Def => (GenStepDef_BspDungeon)def;
        public override int SeedPart => 654321;

        public override void Generate(Map map, GenStepParams parms)
        {
            DungeonGenerator generator = new DungeonGenerator(map, Def);
            generator.Generate();
        }    
    }
}
