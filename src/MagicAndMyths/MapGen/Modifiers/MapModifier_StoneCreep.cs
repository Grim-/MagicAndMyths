using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_StoneCreep : MapModifier
    {
        private readonly TerrainDef stoneTerrain = TerrainDefOf.GraySurface;
        private List<IntVec3> edgeCells = new List<IntVec3>();

        public override int MinTicksBetweenEffects => 500;

        public MapModifier_StoneCreep(Map map) : base(map)
        {
            for (int i = 0; i < 5; i++)
            {
                edgeCells.Add(CellFinder.RandomCell(map));
            }
        }

        public override void ApplyEffect()
        {
            if (!edgeCells.Any()) return;

            // Pick a random edge cell and spread from it
            IntVec3 sourceCell = edgeCells.RandomElement();

            // Get valid adjacent cells
            List<IntVec3> adjacentCells = GenAdj.CellsAdjacent8Way(new TargetInfo(sourceCell, map))
                .Where(c => c.InBounds(map))
                .ToList();

            if (adjacentCells.Any())
            {
                IntVec3 targetCell = adjacentCells.RandomElement();
                map.terrainGrid.SetTerrain(targetCell, stoneTerrain);
                edgeCells.Add(targetCell);
            }

            edgeCells.Remove(sourceCell);
        }
    }

    public class MapModifier_WallGrowth : MapModifier
    {
        private readonly ThingDef wallDef = ThingDefOf.Wall;
        private int currentThickness = 1;
        private readonly int maxThickness;

        public override int MinTicksBetweenEffects => 1000;
        public override int MaxTicksBetweenEffects => 2000;

        public MapModifier_WallGrowth(Map map, int maxThickness = 10) : base(map)
        {
            this.maxThickness = maxThickness;
        }

        public override void ApplyEffect()
        {
            if (currentThickness >= maxThickness) 
                return;

            Messages.Message($"The walls are closing in..", MessageTypeDefOf.NegativeEvent);

            int offset = currentThickness;

            for (int x = offset; x < map.Size.x - offset; x++)
            {
                AddWallIfNeeded(new IntVec3(x, 0, offset));
                AddWallIfNeeded(new IntVec3(x, 0, map.Size.z - offset - 1));
            }

            for (int z = offset; z < map.Size.z - offset; z++)
            {
                AddWallIfNeeded(new IntVec3(offset, 0, z));
                AddWallIfNeeded(new IntVec3(map.Size.x - offset - 1, 0, z));
            }

            currentThickness++;
        }

        private void AddWallIfNeeded(IntVec3 cell)
        {
            if (!cell.InBounds(map)) return;

            // Check if there's already a wall here
            if (cell.GetFirstBuilding(map)?.def == wallDef) 
                return;

            // Clear the cell
            foreach (Thing t in cell.GetThingList(map).ToList())
            {
                t.Destroy();
            }

            // Set terrain and build wall
            map.terrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
            Thing wall = ThingMaker.MakeThing(wallDef, ThingDefOf.Steel);
            GenSpawn.Spawn(wall, cell, map);
        }
    }
}

