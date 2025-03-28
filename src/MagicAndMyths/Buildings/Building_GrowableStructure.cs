using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    [Flags]
    public enum GrowingSkipFlags
    {
        none,
        Walls = 2,
        Doors = 4,
        Floors = 8,
        Power = 16,
        Furniture = 32,
        Other = 64,
        All = 128
    }



    //public class Graphic_DigHole : Graphic_Single
    //{

    //    //lazy init pattern, loads once when needed then uses cache
    //    private Texture2D _DigA;
    //    private Texture2D DigA
    //    {
    //        get
    //        {
    //            if (_DigA == null)
    //            {
    //                _DigA = ContentFinder<Texture2D>.Get("Building/EM_DiggingSpot_a");
    //            }

    //            return _DigA;
    //        }
    //    }

    //    //lazy init pattern
    //    private Texture2D _DigC;
    //    private Texture2D DigC
    //    {
    //        get
    //        {
    //            if (_DigC == null)
    //            {
    //                _DigC = ContentFinder<Texture2D>.Get("Building/EM_DiggingSpot_c");
    //            }

    //            return _DigC;
    //        }
    //    }


    //    //lazy init pattern
    //    private Texture2D _DigB;
    //    private Texture2D DigB
    //    {
    //        get
    //        {
    //            if (_DigB == null)
    //            {
    //                _DigB = ContentFinder<Texture2D>.Get("Building/EM_DiggingSpot_b");
    //            }

    //            return _DigB;
    //        }
    //    }

    //    //besure to change this to your actual buildingType
    //    private Building yourBuilding = null;

    //    public override Material MatSingleFor(Thing thing)
    //    {
    //        Material mat =  base.MatSingleFor(thing);

    //        if (thing is Building yourBuilding)
    //        {
    //            Texture2D texture = null;
    //            switch (State)
    //            {
    //                case DiggingSpotState.Deep:
    //                    texture = DigA;
    //                    break;
    //                case DiggingSpotState.Mid:
    //                    texture = DigB;
    //                    break;
    //                case DiggingSpotState.Top:
    //                    texture = DigC;
    //                    break;
    //            }

    //            if (texture != null)
    //            {
    //                mat.SetTexture("_MainTex", texture);
    //            }
    //        }
    //        return mat;
    //    }
    //}


    public class Building_GrowableStructure : Building
    {
        #region Properties and Fields

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

        private bool includeNaturalTerrain = false;

        private ThingDef overrideWallStuff;
        private TerrainDef overrideFloorStuff;
        private ThingDef overrideDoorStuff;
        private ThingDef overrideFurnitureStuff;

        private ThingFilter ThingFilter;
        private List<Thing> placedThings = new List<Thing>();
        private bool showPreview = true;

        private Color previewColor = Color.green;

        private GrowingSkipFlags skipFlags = GrowingSkipFlags.Floors;

        private List<Color> previewColors = new List<Color>()
        {
            Color.cyan,
            Color.yellow,
            Color.magenta,
            Color.green,
            Color.red,
            Color.blue,
            new Color(1.0f, 0.5f, 0.0f),
            new Color(0.5f, 0.0f, 0.5f),
            new Color(0.0f, 0.5f, 0.5f),
            new Color(1.0f, 0.65f, 0.8f)
        };

        private int currentStagePreviewIndex = 0;
        private Comp_RootGrower RootComp => GetComp<Comp_RootGrower>();

        public IntVec3 TreeCenter
        {
            get
            {
                return new IntVec3(
                Position.x + def.size.x / 2,
                Position.y,
                Position.z + def.size.z / 2);
            }
        }

        public IntVec3 LayoutCenter => new IntVec3(
                Position.x + def.size.x / 2,
                Position.y,
                Position.z + def.size.z / 2
            );

        #endregion

        #region Lifecycle Methods

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                GrowableStructureDef growableDef = def as GrowableStructureDef;
                if (growableDef != null)
                {
                    totalGrowthTicks = growableDef.growthDays * GenDate.TicksPerDay;
                    currentStage = 0;

                    overrideWallStuff = growableDef.defaultWallStuff;
                    overrideFloorStuff = growableDef.defaultFloorStuff;
                    overrideDoorStuff = growableDef.defaultDoorStuff;
                    overrideFurnitureStuff = growableDef.defaultFurnitureStuff;
                }

                ThingFilter = new ThingFilter();
                if (Def != null && Def.disallowedThingFilter != null)
                {
                    ThingFilter.CopyAllowancesFrom(Def.disallowedThingFilter);
                }

               
            }
        }

        public override void Tick()
        {
            base.Tick();

            ticksUntilNextPlacement++;

            if (ticksUntilNextPlacement >= Def.ticksBetweenPlacements)
            {
                ticksUntilNextPlacement = 0;
                GrowTick();
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (!showPreview)
                return;

            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null || currentStage < 0)
                return;

            StructureLayoutDef layout = growableDef.structureLayout;
            List<IntVec3> allPreviewCells = new List<IntVec3>();

            GenDraw.DrawFieldEdges(new List<IntVec3> { Position }, Color.yellow);
            GenDraw.DrawFieldEdges(new List<IntVec3> { TreeCenter }, Color.green);
            GenDraw.DrawFieldEdges(new List<IntVec3> { LayoutCenter }, Color.magenta);

            DrawStructurePreview(layout, allPreviewCells);

            if (allPreviewCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(allPreviewCells, Color.cyan);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode == DestroyMode.KillFinalize)
            {
                DestroyPlacedThings();
            }

            base.Destroy(mode);
        }

        private bool PassesThingFilter(ThingDef thingDef)
        {
            return true;

            if (thingDef == null)
                return false;

            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef == null)
                return true;

            if (ThingFilter != null && !ThingFilter.Allows(thingDef))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Growth and Placement

        private void GrowTick()
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return;

            StructureLayoutDef layout = growableDef.structureLayout;

            int stageCount = layout.stages.Count;
            if (stageCount <= 0)
                return;

            currentGrowthTick++;

            if (AreAllItemsPlaced())
            {
                int targetStage = currentStage + 1;
                targetStage = Mathf.Clamp(targetStage, 0, stageCount - 1);
                if (targetStage > currentStage)
                {
                    SetStage(targetStage);
                    ResetPlacementIndices();
                }
            }
            else
            {
                PlaceNextItem();
            }

            if (IsGrowthComplete())
            {
                FinishGrowth();
            }
        }

        private bool IsGrowthComplete()
        {
            return currentGrowthTick >= totalGrowthTicks && AreAllItemsPlaced();
        }

        private void SetStage(int newIndex)
        {
            currentStage = newIndex;
            SetPreviewIndex(currentStage);
        }

        private void SetPreviewIndex(int newIndex)
        {
            currentStagePreviewIndex = newIndex;
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

            if (stage == null)
            {
                return false;
            }

            // Build in a specific order: terrain -> walls -> doors -> power -> furniture -> other
            if (HasTerrainToBuild(stage))
            {
                BuildTerrain(stage.terrain[nextTerrainIndex]);
                nextTerrainIndex++;
                return true;
            }

            if (HasWallToBuild(stage))
            {
                ThingPlacement placement = stage.walls[nextWallIndex];
                if (PassesThingFilter(placement.thing))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Wall));
                }
                nextWallIndex++;
                return true;
            }

            if (HasDoorToBuild(stage))
            {
                ThingPlacement placement = stage.doors[nextDoorIndex];
                if (PassesThingFilter(placement.thing))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Door));
                }
                nextDoorIndex++;
                return true;
            }

            if (HasPowerToBuild(stage))
            {
                ThingPlacement placement = stage.power[nextPowerIndex];
                if (PassesThingFilter(placement.thing))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Other));
                }
                nextPowerIndex++;
                return true;
            }

            if (HasFurnitureToBuild(stage))
            {
                ThingPlacement placement = stage.furniture[nextFurnitureIndex];
                if (PassesThingFilter(placement.thing))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Furniture));
                }
                nextFurnitureIndex++;
                return true;
            }

            if (HasOtherToBuild(stage))
            {
                ThingPlacement placement = stage.other[nextOtherIndex];
                if (PassesThingFilter(placement.thing))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Other));
                }
                nextOtherIndex++;
                return true;
            }

            return false;
        }

        public void SetIncludeNaturalTerrain(bool include)
        {
            includeNaturalTerrain = include;
        }

        protected virtual bool HasTerrainToBuild(BuildingStage stage)
        {
            return !skipFlags.HasFlag(GrowingSkipFlags.Floors) && nextTerrainIndex < stage.terrain.Count;
        }

        protected virtual bool HasWallToBuild(BuildingStage stage)
        {
            return !skipFlags.HasFlag(GrowingSkipFlags.Walls) && nextWallIndex < stage.walls.Count;
        }

        protected virtual bool HasDoorToBuild(BuildingStage stage)
        {
            return !skipFlags.HasFlag(GrowingSkipFlags.Doors) && nextDoorIndex < stage.doors.Count;
        }

        protected virtual bool HasPowerToBuild(BuildingStage stage)
        {
            return !skipFlags.HasFlag(GrowingSkipFlags.Power) && nextPowerIndex < stage.power.Count;
        }

        protected virtual bool HasFurnitureToBuild(BuildingStage stage)
        {
            return !skipFlags.HasFlag(GrowingSkipFlags.Furniture) && nextFurnitureIndex < stage.furniture.Count;
        }

        protected virtual bool HasOtherToBuild(BuildingStage stage)
        {
            return !skipFlags.HasFlag(GrowingSkipFlags.Other) && nextOtherIndex < stage.other.Count;
        }

        private void BuildTerrain(TerrainPlacement terrain)
        {
            if (terrain.terrain == null)
                return;

            if (!CanBuildTerrain(terrain))
            {
                return;
            }

            IntVec3 pos = CalculateCenteredPosition(terrain.position);

            // Ensure it's within bounds
            if (!pos.InBounds(Map))
                return;

            if (IsCellOccupiedByThisBuilding(pos))
                return;

            TerrainDef terrainToUse = overrideFloorStuff ?? terrain.terrain;

            Map.terrainGrid.SetTerrain(pos, terrainToUse);

            // Visual effect
            FleckMaker.ThrowDustPuff(pos, Map, 0.5f);
        }

        private void BuildThing(ThingPlacement placement, ThingDef stuffOverride = null)
        {
            if (placement.thing == null)
                return;

            IntVec3 pos = CalculateCenteredPosition(placement.position);

            if (!pos.InBounds(Map))
                return;

            if (IsCellOccupiedByThisBuilding(pos) || !CanBuildThing(placement, pos, this.Map))
                return;

            bool canPlace = true;
            List<Thing> existingThings = pos.GetThingList(Map);
            foreach (Thing t in existingThings)
            {
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

            ThingDef stuffToUse = placement.thing.MadeFromStuff ?
                                 (stuffOverride ?? placement.stuff) :
                                 placement.stuff;

            Thing thing = ThingMaker.MakeThing(placement.thing, stuffToUse);

            Thing placedThing = GenSpawn.Spawn(thing, pos, Map, placement.rotation);

            if (placedThing != null)
            {
                placedThings.Add(placedThing);
                FleckMaker.ThrowMetaPuffs(new TargetInfo(pos, Map));
            }
        }

        private IntVec3 CalculateCenteredPosition(IntVec2 relativePos)
        {
            return new IntVec3(
                Position.x + relativePos.x,
                Position.y,
                Position.z + relativePos.z
            );
        }

        private bool IsCellOccupiedByThisBuilding(IntVec3 worldPos)
        {
            List<IntVec3> occupiedCells = new List<IntVec3>();
            this.OccupiedRect().Cells.ToList().ForEach(c => occupiedCells.Add(c));
            return occupiedCells.Contains(worldPos);
        }

        private void FinishGrowth()
        {
            FleckMaker.ThrowLightningGlow(Position.ToVector3Shifted(), Map, 2f);
            MoteMaker.ThrowText(Position.ToVector3Shifted(), Map, "Growth complete", 3.65f);
        }

        private void DestroyPlacedThings()
        {
            foreach (Thing thing in placedThings)
            {
                if (thing != null && !thing.Destroyed)
                {
                    thing.Destroy();
                }
            }

            placedThings.Clear();
        }

        private bool CanBuildTerrain(TerrainPlacement placement)
        {
            if (placement.terrain == null)
                return false;

            if (!includeNaturalTerrain && placement.terrain.natural)
                return false;

            return true;
        }

        private bool CanBuildThing(ThingPlacement placement, IntVec3 pos, Map map)
        {
            // Check if placement thing is valid
            if (placement.thing == null || !placement.thing.BuildableByPlayer)
                return false;

            // Check for any building at this position
            Building existingBuilding = pos.GetFirstBuilding(map);
            if (existingBuilding != null)
            {
                if (existingBuilding.Faction == Faction.OfPlayer)
                    return false;
            }


            foreach (Thing thing in map.thingGrid.ThingsListAt(pos))
            {
                if (thing.def.building?.isNaturalRock == true)
                    return false;
            }

            return true;
        }

        private ThingDef GetMaterialOverride(ThingPlacement placement, BuildingPartType partType)
        {
            if (!placement.thing.MadeFromStuff)
                return null;

            switch (partType)
            {
                case BuildingPartType.Wall:
                    return overrideWallStuff;
                case BuildingPartType.Door:
                    return overrideDoorStuff;
                case BuildingPartType.Furniture:
                    return overrideFurnitureStuff;
                default:
                    return null;
            }
        }

        #endregion

        #region Drawing and Visualization

        private void DrawStructurePreview(StructureLayoutDef layout, List<IntVec3> allPreviewCells)
        {
            //draw them all
            if (currentStagePreviewIndex == -1)
            {
                for (int i = 0; i < layout.stages.Count; i++)
                {
                    int colorIndex = i % previewColors.Count;
                    Color startColor = previewColors[colorIndex];
                    BuildingStage currentBStage = layout.stages[i];

                    StructurePreviewUtility.DrawThingPreviews(currentBStage.walls, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(currentBStage.doors, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(currentBStage.power, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(currentBStage.furniture, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(currentBStage.other, Position, Rotation, Map, startColor, allPreviewCells);
                }
            }
            else
            {
                int previewStage = Mathf.Clamp(currentStagePreviewIndex, 0, layout.stages.Count - 1);
                BuildingStage currentBStage = layout.stages[previewStage];

                StructurePreviewUtility.DrawThingPreviews(currentBStage.walls, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(currentBStage.doors, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(currentBStage.power, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(currentBStage.furniture, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(currentBStage.other, Position, Rotation, Map, previewColor, allPreviewCells);
            }
        }

        #endregion


        public void SetSkipFlags(GrowingSkipFlags flags)
        {
            skipFlags = flags;
        }

        public void ToggleSkipFlag(GrowingSkipFlags flag)
        {
            skipFlags ^= flag;
        }

        #region Gizmos and Inspection

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            // Stage preview controls (already implemented)
            yield return new Command_Action()
            {
                defaultLabel = "Change stage preview",
                action = () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    for (int i = 0; i < Def.structureLayout.stages.Count; i++)
                    {
                        if (i != currentStagePreviewIndex)
                        {
                            int stageIndex = i;
                            options.Add(new FloatMenuOption($"Stage {i + 1}", () =>
                            {
                                SetPreviewIndex(stageIndex);
                            }));
                        }
                    }

                    if (currentStagePreviewIndex != -1)
                    {
                        options.Add(new FloatMenuOption($"Show all stages", () =>
                        {
                            SetPreviewIndex(-1);
                        }));
                    }

                    options.Add(new FloatMenuOption($"Toggle preview {(showPreview ? "off" : "on")}", () =>
                    {
                        showPreview = !showPreview;
                    }));

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
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

        #endregion

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

            Scribe_Values.Look(ref showPreview, "showPreview", true);
            Scribe_Values.Look(ref currentStagePreviewIndex, "currentStagePreviewIndex", 0);

            Scribe_Values.Look(ref skipFlags, "skipFlags", GrowingSkipFlags.none);
            Scribe_Values.Look(ref includeNaturalTerrain, "includeNaturalTerrain", false);

            Scribe_Defs.Look(ref overrideWallStuff, "overrideWallStuff");
            Scribe_Defs.Look(ref overrideFloorStuff, "overrideFloorStuff");
            Scribe_Defs.Look(ref overrideDoorStuff, "overrideDoorStuff");
            Scribe_Defs.Look(ref overrideFurnitureStuff, "overrideFurnitureStuff");

            Scribe_Deep.Look(ref ThingFilter, "thingFilter");

            Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Reference);
        }
    }


    public enum BuildingPartType
    {
        Wall,
        Door,
        Furniture,
        Floor,
        Other
    }

}
