using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DomainZoneDef : ActiveZoneDef
    {
        public TerrainDef FloorDef;
        public ThingDef WallDef;
        public ThingDef FilthDef;
        public bool ConstructWalls = true;
        public bool ReplaceFloor = true;
        public bool CreateFilth = false;

        public DomainZoneDef()
        {
            thingClass = typeof(DomainActiveZone);
        }
    }

    public class DomainActiveZone : ActiveZone
    {
        public new DomainZoneDef ActiveZoneDef => (DomainZoneDef)def;

        protected HashSet<IntVec3> wallCells;
        protected List<Thing> constructedWalls;
        private HashSet<IntVec3> AddedFilth = new HashSet<IntVec3>();

        private Dictionary<IntVec3, TerrainDef> originalTerrain = new Dictionary<IntVec3, TerrainDef>();
        private Dictionary<IntVec3, List<Thing>> originalThings = new Dictionary<IntVec3, List<Thing>>();
        private Dictionary<IntVec3, List<Thing>> originalWallCellContents = new Dictionary<IntVec3, List<Thing>>();
        private bool terrainChanged = false;
        private bool wallsConstructed = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (ZoneCells == null || ZoneCells.Count == 0)
            {
                return;
            }

            if (ActiveZoneDef.ConstructWalls)
            {
                CalculateWallCells();
                ConstructWalls();
            }

            if (ActiveZoneDef.ReplaceFloor)
            {
                ChangeTerrain();
            }

            if (ActiveZoneDef.CreateFilth)
            {
                CreateFilth();
            }

            if (respawningAfterLoad && terrainChanged)
            {
                ReapplyTerrainChanges();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            RemoveWalls();
            RemoveFilth();
            RevertTerrain();
            base.DeSpawn(mode);
        }

        protected virtual void CalculateWallCells()
        {
            if (Map == null || ZoneCells == null || ZoneCells.Count == 0)
            {
                return;
            }

            wallCells = new HashSet<IntVec3>();
            HashSet<IntVec3> zoneCellsSet = new HashSet<IntVec3>(ZoneCells);

            foreach (IntVec3 cell in ZoneCells)
            {
                bool isEdge = false;
                foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell))
                {
                    if (!zoneCellsSet.Contains(neighbor))
                    {
                        isEdge = true;
                        break;
                    }
                }

                if (isEdge && cell.InBounds(Map))
                {
                    wallCells.Add(cell);
                }
            }
        }

        private void ChangeTerrain()
        {
            if (ActiveZoneDef.FloorDef != null)
            {
                foreach (IntVec3 cell in ZoneCells)
                {
                    if (cell.InBounds(Map))
                    {
                        TerrainDef currentTerrain = cell.GetTerrain(Map);
                        if (!originalTerrain.ContainsKey(cell))
                        {
                            originalTerrain[cell] = currentTerrain;
                        }

                        List<Thing> cellThings = cell.GetThingList(Map)
                            .Where(t => t.def.category == ThingCategory.Plant)
                            .ToList();
                        if (cellThings.Any())
                        {
                            originalThings[cell] = cellThings;
                            foreach (Thing thing in cellThings)
                            {
                                thing.DeSpawn();
                            }
                        }

                        Map.terrainGrid.SetTerrain(cell, ActiveZoneDef.FloorDef);
                    }
                }
                terrainChanged = true;
            }


        }

        private void RevertTerrain()
        {
            if (terrainChanged)
            {
                foreach (var kvp in originalTerrain)
                {
                    IntVec3 cell = kvp.Key;
                    TerrainDef terrain = kvp.Value;

                    if (cell.InBounds(Map))
                    {
                        Map.terrainGrid.SetTerrain(cell, terrain);

                        if (originalThings.TryGetValue(cell, out List<Thing> cellThings))
                        {
                            foreach (Thing thing in cellThings)
                            {
                                if (!thing.Spawned)
                                {
                                    GenSpawn.Spawn(thing, cell, Map);
                                }
                            }
                        }
                    }
                }
                originalTerrain.Clear();
                originalThings.Clear();
                terrainChanged = false;
            }
        }

        private void ReapplyTerrainChanges()
        {
            if (ActiveZoneDef.FloorDef != null)
            {
                foreach (var kvp in originalTerrain)
                {
                    if (kvp.Key.InBounds(Map))
                    {
                        Map.terrainGrid.SetTerrain(kvp.Key, ActiveZoneDef.FloorDef);
                    }
                }
            }


        }

        private void CreateFilth()
        {
            if (ActiveZoneDef.FilthDef != null)
            {
                AddedFilth.Clear();
                foreach (IntVec3 cell in ZoneCells)
                {
                    if (FilthMaker.CanMakeFilth(cell, Map, ActiveZoneDef.FilthDef))
                    {
                        if (FilthMaker.TryMakeFilth(cell, Map, ActiveZoneDef.FilthDef))
                        {
                            AddedFilth.Add(cell);
                        }
                    }
                }
            }
        }

        private void RemoveFilth()
        {
            foreach (IntVec3 cell in AddedFilth)
            {
                FilthMaker.RemoveAllFilth(cell, Map);
            }
            AddedFilth.Clear();
        }

        public virtual void ConstructWalls()
        {
            if (ActiveZoneDef.WallDef != null)
            {
                CalculateWallCells();
                constructedWalls = new List<Thing>();

                foreach (IntVec3 cell in wallCells)
                {
                    if (cell.InBounds(Map))
                    {
                        List<Thing> cellContents = cell.GetThingList(Map)
                            .Where(t => t.def.category != ThingCategory.Pawn)
                            .ToList();
                        if (cellContents.Any())
                        {
                            originalWallCellContents[cell] = cellContents;
                            foreach (Thing thing in cellContents)
                            {
                                thing.DeSpawn();
                            }
                        }

                        Thing wall = ThingMaker.MakeThing(ActiveZoneDef.WallDef);
                        if (wall != null)
                        {
                            Thing spawnedWall = GenSpawn.Spawn(wall, cell, Map);
                            if (spawnedWall != null)
                            {
                                constructedWalls.Add(spawnedWall);
                            }
                        }
                    }
                }
                wallsConstructed = true;
            }
        }

        public virtual void RemoveWalls()
        {
            if (wallsConstructed)
            {
                foreach (Thing wall in constructedWalls)
                {
                    if (!wall.Destroyed)
                    {
                        wall.Destroy();
                    }
                }
                constructedWalls.Clear();

                foreach (var kvp in originalWallCellContents)
                {
                    IntVec3 cell = kvp.Key;
                    List<Thing> cellContents = kvp.Value;

                    if (cell.InBounds(Map))
                    {
                        foreach (Thing thing in cellContents)
                        {
                            if (!thing.Spawned)
                            {
                                GenSpawn.Spawn(thing, cell, Map);
                            }
                        }
                    }
                }
                originalWallCellContents.Clear();
                wallsConstructed = false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref constructedWalls, "constructedWalls", LookMode.Reference);
            Scribe_Collections.Look(ref originalTerrain, "originalTerrain", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look(ref originalWallCellContents, "originalWallCellContents", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref wallsConstructed, "wallsConstructed", false);
            Scribe_Collections.Look(ref originalThings, "originalThings", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref terrainChanged, "terrainChanged", false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Vector3> wallPositions = wallCells?.Select(c => c.ToVector3()).ToList();
                Scribe_Collections.Look(ref wallPositions, "wallCells", LookMode.Value);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<Vector3> wallPositions = null;
                Scribe_Collections.Look(ref wallPositions, "wallCells", LookMode.Value);
                if (wallPositions != null)
                {
                    wallCells = new HashSet<IntVec3>(wallPositions.Select(v => v.ToIntVec3()));
                }
            }
        }

        public static DomainActiveZone SpawnTerrainZone(DomainZoneDef activeZoneDef, IntVec3 SpawnPosition, List<IntVec3> ZoneCells, Map map)
        {
            if (activeZoneDef == null)
            {
                return null;
            }

            if (ZoneCells.NullOrEmpty())
            {
                return null;
            }

            DomainActiveZone zone = (DomainActiveZone)ThingMaker.MakeThing(activeZoneDef);
            zone.ZoneLifeTime = activeZoneDef.ZoneLifeTime;
            zone.SetZoneCells(ZoneCells);
            zone = (DomainActiveZone)GenSpawn.Spawn(zone, SpawnPosition, map);
            return zone;
        }
    }
}