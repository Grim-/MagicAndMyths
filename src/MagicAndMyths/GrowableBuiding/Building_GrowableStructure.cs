using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class StructureLayoutDef : Def
    {
        // Structure size
        public IntVec2 size;

        // List of all building stages
        public List<BuildingStage> stages = new List<BuildingStage>();
    }

    public class BuildingStage
    {
        // All the things to place in this stage
        public List<TerrainPlacement> terrain = new List<TerrainPlacement>();
        public List<ThingPlacement> walls = new List<ThingPlacement>();
        public List<ThingPlacement> doors = new List<ThingPlacement>();
        public List<ThingPlacement> power = new List<ThingPlacement>();
        public List<ThingPlacement> furniture = new List<ThingPlacement>();
        public List<ThingPlacement> other = new List<ThingPlacement>();
    }

    public class TerrainPlacement
    {
        public TerrainDef terrain;
        public IntVec2 position;
    }

    // Class for thing placement
    public class ThingPlacement
    {
        public ThingDef thing;
        public IntVec2 position;
        public Rot4 rotation = Rot4.North;
        public ThingDef stuff;
    }
    public class GrowableStructureDef : ThingDef
    {
        public StructureLayoutDef structureLayout;
        public int growthDays = 3;

        public int ticksBetweenPlacements = 50;

    }
    public class Building_GrowableStructure : Building
    {
        GrowableStructureDef Def => (GrowableStructureDef)def;

        // Growth tracking
        private int currentGrowthTick = 0;
        private int totalGrowthTicks = 0;
        private int currentStage = -1;

        // Item tracking
        private int nextTerrainIndex = 0;
        private int nextWallIndex = 0;
        private int nextDoorIndex = 0;
        private int nextPowerIndex = 0;
        private int nextFurnitureIndex = 0;
        private int nextOtherIndex = 0;

        private int ticksUntilNextPlacement = 0;

        private List<Thing> placedThings = new List<Thing>();
        private bool showPreview = true;
        private HashSet<IntVec3> previewCells = new HashSet<IntVec3>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                GrowableStructureDef growableDef = def as GrowableStructureDef;
                if (growableDef != null)
                {
                    // Calculate total growth time
                    totalGrowthTicks = growableDef.growthDays * GenDate.TicksPerDay;

                    // Start with first stage
                    currentStage = 0;
                }
            }
        }

        // Regular update tick
        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame % 10 == 0) 
            {
                GrowTick();
            }
        }
        private void GeneratePreviewCells()
        {
            previewCells.Clear();

            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return;

            StructureLayoutDef layout = growableDef.structureLayout;

            // Process all stages to collect every cell that will be used
            foreach (BuildingStage stage in layout.stages)
            {
                // Add terrain cells
                foreach (TerrainPlacement terrain in stage.terrain)
                {
                    IntVec3 pos = CalculateCenteredPosition(terrain.position);
                    if (pos.InBounds(Map) && !IsCellOccupiedByThisBuilding(pos))
                    {
                        previewCells.Add(pos);
                    }
                }

                // Add building cells from all categories
                AddThingPreviewCells(stage.walls);
                AddThingPreviewCells(stage.doors);
                AddThingPreviewCells(stage.power);
                AddThingPreviewCells(stage.furniture);
                AddThingPreviewCells(stage.other);
            }
        }

        // Helper method to add thing positions to preview
        private void AddThingPreviewCells(List<ThingPlacement> things)
        {
            foreach (ThingPlacement thing in things)
            {
                IntVec3 pos = CalculateCenteredPosition(thing.position);
                if (pos.InBounds(Map) && !IsCellOccupiedByThisBuilding(pos))
                {
                    previewCells.Add(pos);
                }
            }
        }
        public override void Notify_ThingSelected()
        {
            base.Notify_ThingSelected();
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            GeneratePreviewCells();

            if (showPreview && previewCells.Count > 0)
            {
                // Draw outline for all cells that will be occupied
                GenDraw.DrawFieldEdges(previewCells.ToList(), Color.cyan);

                // Draw the center point (this building) in a different color
                GenDraw.DrawFieldEdges(new List<IntVec3> { Position }, Color.yellow);
            }
        }
        private void GrowTick()
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return;
            currentGrowthTick += 10;
            StructureLayoutDef layout = growableDef.structureLayout;

            int stageCount = layout.stages.Count;
            if (stageCount <= 0)
                return;

            int targetStage = Mathf.FloorToInt((float)currentGrowthTick / totalGrowthTicks * stageCount);
            targetStage = Mathf.Clamp(targetStage, 0, stageCount - 1);
            if (targetStage > currentStage)
            {
                currentStage = targetStage;
                ResetPlacementIndices();
            }

            if (ticksUntilNextPlacement > 0)
            {
                ticksUntilNextPlacement -= 10;
            }

            if (ticksUntilNextPlacement <= 0)
            {
                if (PlaceNextItem())
                {
                    ticksUntilNextPlacement = Def.ticksBetweenPlacements;
                }
            }

            // If we've finished growing
            if (currentGrowthTick >= totalGrowthTicks && AreAllItemsPlaced())
            {
                FinishGrowth();
            }
        }
        private void ResetPlacementIndices()
        {
            nextTerrainIndex = 0;
            nextWallIndex = 0;
            nextDoorIndex = 0;
            nextPowerIndex = 0;
            nextFurnitureIndex = 0;
            nextOtherIndex = 0;
        }

        private bool AreAllItemsPlaced()
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null || currentStage < 0)
                return true;

            BuildingStage stage = growableDef.structureLayout.stages[currentStage];

            return
                nextTerrainIndex >= stage.terrain.Count &&
                nextWallIndex >= stage.walls.Count &&
                nextDoorIndex >= stage.doors.Count &&
                nextPowerIndex >= stage.power.Count &&
                nextFurnitureIndex >= stage.furniture.Count &&
                nextOtherIndex >= stage.other.Count;
        }

   
        private bool PlaceNextItem()
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null || currentStage < 0)
                return false;

            BuildingStage stage = growableDef.structureLayout.stages[currentStage];

            // Build in a specific order: terrain -> walls -> doors -> power -> furniture -> other

            // Try to place terrain first
            if (nextTerrainIndex < stage.terrain.Count)
            {
                BuildTerrain(stage.terrain[nextTerrainIndex]);
                nextTerrainIndex++;
                return true;
            }

            // Then walls
            if (nextWallIndex < stage.walls.Count)
            {
                BuildThing(stage.walls[nextWallIndex]);
                nextWallIndex++;
                return true;
            }

            // Then doors
            if (nextDoorIndex < stage.doors.Count)
            {
                BuildThing(stage.doors[nextDoorIndex]);
                nextDoorIndex++;
                return true;
            }

            // Then power
            if (nextPowerIndex < stage.power.Count)
            {
                BuildThing(stage.power[nextPowerIndex]);
                nextPowerIndex++;
                return true;
            }

            // Then furniture
            if (nextFurnitureIndex < stage.furniture.Count)
            {
                BuildThing(stage.furniture[nextFurnitureIndex]);
                nextFurnitureIndex++;
                return true;
            }

            // Finally other items
            if (nextOtherIndex < stage.other.Count)
            {
                BuildThing(stage.other[nextOtherIndex]);
                nextOtherIndex++;
                return true;
            }

            // Nothing left to place
            return false;
        }


        private void BuildTerrain(TerrainPlacement terrain)
        {
            if (terrain.terrain == null)
                return;

            // Calculate world position, centered around this building
            IntVec3 pos = CalculateCenteredPosition(terrain.position);

            // Ensure it's within bounds
            if (!pos.InBounds(Map))
                return;

            // Skip if this would modify the terrain under the origin building
            if (IsCellOccupiedByThisBuilding(pos))
                return;

            // Set the terrain
            Map.terrainGrid.SetTerrain(pos, terrain.terrain);

            // Visual effect
            FleckMaker.ThrowDustPuff(pos, Map, 0.5f);
        }


        private void BuildThing(ThingPlacement placement)
        {
            if (placement.thing == null)
                return;

            // Calculate world position, centered around this building
            IntVec3 pos = CalculateCenteredPosition(placement.position);

            // Ensure it's within bounds
            if (!pos.InBounds(Map))
                return;

            // Skip if this would place something in the same cell as the origin building
            if (IsCellOccupiedByThisBuilding(pos))
                return;

            // Check if something blocking is already there
            bool canPlace = true;
            List<Thing> existingThings = pos.GetThingList(Map);
            foreach (Thing t in existingThings)
            {
                // Skip if this is the origin building itself
                if (t == this)
                    continue;

                if (t.def == placement.thing || t.def.entityDefToBuild == placement.thing)
                {
                    canPlace = false;
                    break;
                }
            }

            if (!canPlace)
                return;

            // Create the thing
            Thing thing = ThingMaker.MakeThing(placement.thing, placement.stuff);

            // Spawn it
            Thing placedThing = GenSpawn.Spawn(thing, pos, Map, placement.rotation);

            // Track it
            if (placedThing != null)
            {
                placedThings.Add(placedThing);

                // Visual and sound effects
                FleckMaker.ThrowMetaPuffs(new TargetInfo(pos, Map));
            }
        }

        private IntVec3 CalculateCenteredPosition(IntVec2 relativePos)
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return Position + new IntVec3(relativePos.x, 0, relativePos.z);

            // Get the center point based on the structure's size
            IntVec2 halfSize = new IntVec2(
                Mathf.FloorToInt(growableDef.structureLayout.size.x / 2f),
                Mathf.FloorToInt(growableDef.structureLayout.size.z / 2f)
            );

            // Offset the relative position by half the structure size to center it
            return Position + new IntVec3(relativePos.x - halfSize.x, 0, relativePos.z - halfSize.z);
        }

        private bool IsCellOccupiedByThisBuilding(IntVec3 worldPos)
        {
            // Get the occupied cells of this building
            List<IntVec3> occupiedCells = new List<IntVec3>();
            this.OccupiedRect().Cells.ToList().ForEach(c => occupiedCells.Add(c));

            // Check if the target position is within our occupied cells
            return occupiedCells.Contains(worldPos);
        }

        private void FinishGrowth()
        {
            FleckMaker.ThrowLightningGlow(Position.ToVector3Shifted(), Map, 2f);
            MoteMaker.ThrowText(Position.ToVector3Shifted(), Map, "Growth complete", 3.65f);

            //this.Destroy();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode == DestroyMode.KillFinalize)
            {
                foreach (Thing thing in placedThings)
                {
                    if (thing != null && !thing.Destroyed)
                    {
                        thing.Destroy();
                    }
                }
            }

            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(base.GetInspectString()))
            {
                sb.AppendLine(base.GetInspectString());
            }

            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout != null)
            {
                float percentComplete = (float)currentGrowthTick / totalGrowthTicks * 100f;
                int stageCount = growableDef.structureLayout.stages.Count;

                if (stageCount > 0 && currentStage >= 0)
                {
                    sb.AppendLine("Growth: " + percentComplete.ToString("F0") + "% complete");
                    sb.AppendLine("Stage: " + (currentStage + 1) + "/" + stageCount);

                    // Calculate nd show items remaining in current stage
                    BuildingStage stage = growableDef.structureLayout.stages[currentStage];
                    int itemsTotal = stage.terrain.Count + stage.walls.Count + stage.doors.Count +
                                    stage.power.Count + stage.furniture.Count + stage.other.Count;

                    int itemsPlaced = nextTerrainIndex + nextWallIndex + nextDoorIndex +
                                     nextPowerIndex + nextFurnitureIndex + nextOtherIndex;

                    sb.AppendLine("Items placed: " + itemsPlaced + "/" + itemsTotal);
                    sb.AppendLine("Time remaining: " + ((totalGrowthTicks - currentGrowthTick) / GenTicks.TicksPerRealSecond).ToStringTicksToPeriod());
                }
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentGrowthTick, "currentGrowthTick", 0);
            Scribe_Values.Look(ref totalGrowthTicks, "totalGrowthTicks", 0);
            Scribe_Values.Look(ref currentStage, "currentStage", -1);

            Scribe_Values.Look(ref nextTerrainIndex, "nextTerrainIndex", 0);
            Scribe_Values.Look(ref nextWallIndex, "nextWallIndex", 0);
            Scribe_Values.Look(ref nextDoorIndex, "nextDoorIndex", 0);
            Scribe_Values.Look(ref nextPowerIndex, "nextPowerIndex", 0);
            Scribe_Values.Look(ref nextFurnitureIndex, "nextFurnitureIndex", 0);
            Scribe_Values.Look(ref nextOtherIndex, "nextOtherIndex", 0);
            Scribe_Values.Look(ref ticksUntilNextPlacement, "ticksUntilNextPlacement", 0);

            Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Reference);
        }
    }
}
