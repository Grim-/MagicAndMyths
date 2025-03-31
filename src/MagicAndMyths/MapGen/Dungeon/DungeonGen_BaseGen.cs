using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGen_BaseGen : DungeonGen
    {
        public override int Priority => 10;

        public DungeonGen_BaseGen(Map map) : base(map) { }

        public override void Generate()
        {
            CellRect rect = new CellRect(4, 4, map.Size.x - 8, map.Size.z - 8);

            var resolveParams = new ResolveParams
            {
                rect = rect,
                faction = Faction.OfAncientsHostile,
                wallStuff = ThingDefOf.Steel,
                floorDef = TerrainDefOf.MetalTile
            };

            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("ancientTemple", resolveParams);
            BaseGen.Generate();
        }
    }
}

