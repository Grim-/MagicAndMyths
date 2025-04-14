using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{

    public class GenStep_BspDungeon : GenStep
    {
        GenStepDef_BspDungeon Def => (GenStepDef_BspDungeon)def;
        public override int SeedPart => 654321;

        public override void Generate(Map map, GenStepParams parms)
        {
            DungeonGenerator generator = new DungeonGenerator(map, Def.dungeonGenDef);
            generator.Generate();
        }    
    }
}
