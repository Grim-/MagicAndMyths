using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class MapInitializationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            FillMapWithWalls(context);
        }

        private void FillMapWithWalls(DungeonGenerationContext context)
        {
            foreach (IntVec3 cell in context.Map.AllCells)
            {
                Thing thing = ThingMaker.MakeThing(
                    context.Def.WallDef,
                    context.Def.WallStuffDef ?? GenStuff.DefaultStuffFor(context.Def.WallDef));
                GenSpawn.Spawn(thing, cell, context.Map);
                context.Map.terrainGrid.SetUnderTerrain(cell, context.Def.TerrainDef);
            }
        }
    }
}
