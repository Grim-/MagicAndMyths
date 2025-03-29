using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Building_GrowableStructure : Building
    {
        #region Properties and Fields
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
        private List<Thing> lastStageThings = new List<Thing>();
        private bool showPreview = true;
        private bool removeLastStageOnProgress = true;

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


        public GrowableStructureDef Def => (GrowableStructureDef)def;
        public StructureLayoutDef LayoutDef => this.Def.structureLayout;



        //TREE LIFE CYCLE
        //SPROUT = first 'planted'
        //BLOOM -> WITHERING
        //WITHERING -> WITHERING
        //DEAD = no growth anymore

        #endregion

        #region Lifecycle Methods
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                Init(Def);
            }
        }


        private void Init(GrowableStructureDef growableDef)
        {
            totalGrowthTicks = growableDef.growthDays * GenDate.TicksPerDay;
            currentStage = 0;
            overrideWallStuff = growableDef.defaultWallStuff;
            overrideFloorStuff = growableDef.defaultFloorStuff;
            overrideDoorStuff = growableDef.defaultDoorStuff;
            overrideFurnitureStuff = growableDef.defaultFurnitureStuff;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode == DestroyMode.KillFinalize)
            {
                DestroyPlacedThings();
            }

            base.Destroy(mode);
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
        #endregion

        #region Growth

        private void GrowTick()
        {
            if (LayoutDef == null)
                return;

            int stageCount = LayoutDef.stages.Count;
            if (stageCount <= 0 || currentStage >= stageCount)
                return;

            currentGrowthTick++;

            if (IsStageFullyBuilt())
            {
                OnStageBuildComplete(currentStage);
                int targetStage = currentStage + 1;

                if (targetStage >= LayoutDef.stages.Count)
                {
                    //finished
                    return;
                }
                else
                {
                    if (removeLastStageOnProgress && lastStageThings.Count > 0)
                    {
                        DestroyLastStageThings();
                    }
                    else
                    {
                        // Clear the list without destroying things
                        lastStageThings.Clear();
                    }

                    SetTargetStage(targetStage);
                    OnStageStarted(currentStage);
                }
            }
            else
            {
                BuildNext();
            }

            if (IsGrowthComplete())
            {
                OnFinishedGrowing();
            }
        }
        private void OnFinishedGrowing()
        {
            FleckMaker.ThrowLightningGlow(Position.ToVector3Shifted(), Map, 2f);
            MoteMaker.ThrowText(Position.ToVector3Shifted(), Map, "Growth complete", 3.65f);
        }
        private bool IsGrowthComplete()
        {
            return IsStageFullyBuilt();
        }

        public void DoRegenTick()
        {
            List<Thing> damagedBuiltThings = placedThings.Where(x => x.def.useHitPoints && x.HitPoints < x.MaxHitPoints).ToList();
            Thing nextThingToRepair = damagedBuiltThings.Last();
            if (nextThingToRepair != null)
            {
                nextThingToRepair.HitPoints += 1;
            }
        }
        public void DoDegenTick()
        {
            if (placedThings != null && placedThings.Count > 0)
            {
                Thing nextThingToDamage = placedThings.Last();
                if (nextThingToDamage != null)
                {
                    nextThingToDamage.HitPoints -= 1;

                    if (nextThingToDamage.DestroyedOrNull())
                    {
                        placedThings.RemoveWhere(x => x == null);
                    }
                }
            }
        }

        #endregion

        #region Stage

        private void SetStage(int newIndex)
        {
            currentStage = Mathf.Clamp(newIndex, 0, LayoutDef.stages.Count - 1);
            SetPreviewIndex(currentStage);
        }
        private void OnStageStarted(int stageIndex)
        {
            Messages.Message($"{this.Label} has started building stage {stageIndex}", MessageTypeDefOf.PositiveEvent);
            ResetPlacementIndices();
        }
        private void OnStageBuildComplete(int stageIndex)
        {
            Messages.Message($"{this.Label} has finished building stage {stageIndex}", MessageTypeDefOf.PositiveEvent);
        }
        private void SetTargetStage(int targetStage)
        {
            targetStage = Mathf.Clamp(targetStage, 0, LayoutDef.stages.Count - 1);
            if (targetStage > currentStage)
            {
                SetStage(targetStage);
            }
        }

        #endregion

        #region Building
        public void SetSkipFlags(GrowingSkipFlags flags)
        {
            skipFlags = flags;
        }
        public void ToggleSkipFlag(GrowingSkipFlags flag)
        {
            skipFlags ^= flag;
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
        private bool IsStageFullyBuilt()
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (LayoutDef == null || currentStage < 0)
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
        private bool BuildNext()
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
                if (!skipFlags.HasFlag(GrowingSkipFlags.Floors))
                {
                    BuildTerrain(stage.terrain[nextTerrainIndex]);
                }
                nextTerrainIndex++;
                return true;
            }

            if (HasWallToBuild(stage))
            {
                ThingPlacement placement = stage.walls[nextWallIndex];
                if (!skipFlags.HasFlag(GrowingSkipFlags.Walls))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Wall));
                }
                nextWallIndex++;
                return true;
            }

            if (HasDoorToBuild(stage))
            {
                ThingPlacement placement = stage.doors[nextDoorIndex];
                if (!skipFlags.HasFlag(GrowingSkipFlags.Doors))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Door));
                }
                nextDoorIndex++;
                return true;
            }

            if (HasPowerToBuild(stage))
            {
                ThingPlacement placement = stage.power[nextPowerIndex];
                if (!skipFlags.HasFlag(GrowingSkipFlags.Power))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Other));
                }
                nextPowerIndex++;
                return true;
            }

            if (HasFurnitureToBuild(stage))
            {
                ThingPlacement placement = stage.furniture[nextFurnitureIndex];
                if (!skipFlags.HasFlag(GrowingSkipFlags.Furniture))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Furniture));
                }
                nextFurnitureIndex++;
                return true;
            }

            if (HasOtherToBuild(stage))
            {
                ThingPlacement placement = stage.other[nextOtherIndex];
                if (!skipFlags.HasFlag(GrowingSkipFlags.Other))
                {
                    BuildThing(placement, GetMaterialOverride(placement, BuildingPartType.Other));
                }
                nextOtherIndex++;
                return true;
            }

            return false;
        }
        public void DestroyLastPlaced()
        {
            if (placedThings != null && placedThings.Count > 0)
            {
                placedThings.Last().Destroy(DestroyMode.Vanish);
                placedThings.RemoveLast();
            }
        }

        public void DestroyLastStageThings()
        {
            foreach (Thing thing in lastStageThings)
            {
                if (thing != null && !thing.Destroyed)
                {
                    placedThings.Remove(thing);
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            lastStageThings.Clear();
        }

        public void SetRemoveLastStageOnProgress(bool remove)
        {
            removeLastStageOnProgress = remove;
        }

        public void SetIncludeNaturalTerrain(bool include)
        {
            includeNaturalTerrain = include;
        }

        protected virtual bool HasTerrainToBuild(BuildingStage stage)
        {
            return nextTerrainIndex < stage.terrain.Count;
        }
        protected virtual bool HasWallToBuild(BuildingStage stage)
        {
            return nextWallIndex < stage.walls.Count;
        }
        protected virtual bool HasDoorToBuild(BuildingStage stage)
        {
            return nextDoorIndex < stage.doors.Count;
        }
        protected virtual bool HasPowerToBuild(BuildingStage stage)
        {
            return nextPowerIndex < stage.power.Count;
        }
        protected virtual bool HasFurnitureToBuild(BuildingStage stage)
        {
            return nextFurnitureIndex < stage.furniture.Count;
        }
        protected virtual bool HasOtherToBuild(BuildingStage stage)
        {
            return nextOtherIndex < stage.other.Count;
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
            if (!pos.InBounds(Map))
                return;

            if (IsCellOccupiedByThisBuilding(pos))
                return;

            TerrainDef terrainToUse = overrideFloorStuff ?? terrain.terrain;
            Map.terrainGrid.SetTerrain(pos, terrainToUse);
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
                lastStageThings.Add(placedThing);
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
            lastStageThings.Clear();
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
            if (placement.thing == null || !placement.thing.BuildableByPlayer)
                return false;

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
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (!showPreview)
                return;

            if (LayoutDef == null || currentStage < 0)
                return;

            List<IntVec3> allPreviewCells = new List<IntVec3>();

            GenDraw.DrawFieldEdges(new List<IntVec3> { Position }, Color.yellow);
            GenDraw.DrawFieldEdges(new List<IntVec3> { TreeCenter }, Color.green);
            GenDraw.DrawFieldEdges(new List<IntVec3> { LayoutCenter }, Color.magenta);

            DrawStructurePreview(LayoutDef, allPreviewCells);

            if (allPreviewCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(allPreviewCells, Color.cyan);
            }
        }
        private void SetPreviewIndex(int newIndex)
        {
            currentStagePreviewIndex = newIndex;
        }
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

                    List<ThingPlacement> unbuiltWalls = FilterUnbuiltPlacements(currentBStage.walls);
                    List<ThingPlacement> unbuiltDoors = FilterUnbuiltPlacements(currentBStage.doors);
                    List<ThingPlacement> unbuiltPower = FilterUnbuiltPlacements(currentBStage.power);
                    List<ThingPlacement> unbuiltFurniture = FilterUnbuiltPlacements(currentBStage.furniture);
                    List<ThingPlacement> unbuiltOther = FilterUnbuiltPlacements(currentBStage.other);

                    StructurePreviewUtility.DrawThingPreviews(unbuiltWalls, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltDoors, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltPower, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltFurniture, Position, Rotation, Map, startColor, allPreviewCells);
                    StructurePreviewUtility.DrawThingPreviews(unbuiltOther, Position, Rotation, Map, startColor, allPreviewCells);
                }
            }
            else
            {
                int previewStage = Mathf.Clamp(currentStagePreviewIndex, 0, layout.stages.Count - 1);
                BuildingStage currentBStage = layout.stages[previewStage];

                List<ThingPlacement> unbuiltWalls = FilterUnbuiltPlacements(currentBStage.walls);
                List<ThingPlacement> unbuiltDoors = FilterUnbuiltPlacements(currentBStage.doors);
                List<ThingPlacement> unbuiltPower = FilterUnbuiltPlacements(currentBStage.power);
                List<ThingPlacement> unbuiltFurniture = FilterUnbuiltPlacements(currentBStage.furniture);
                List<ThingPlacement> unbuiltOther = FilterUnbuiltPlacements(currentBStage.other);

                StructurePreviewUtility.DrawThingPreviews(unbuiltWalls, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltDoors, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltPower, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltFurniture, Position, Rotation, Map, previewColor, allPreviewCells);
                StructurePreviewUtility.DrawThingPreviews(unbuiltOther, Position, Rotation, Map, previewColor, allPreviewCells);
            }
        }
        private List<ThingPlacement> FilterUnbuiltPlacements(List<ThingPlacement> placements)
        {
            List<ThingPlacement> unbuiltPlacements = new List<ThingPlacement>();

            foreach (ThingPlacement placement in placements)
            {
                IntVec3 pos = CalculateCenteredPosition(placement.position);

                if (!pos.InBounds(Map))
                    continue;
                bool isBuilt = false;
                List<Thing> thingsAt = pos.GetThingList(Map);
                foreach (Thing thing in thingsAt)
                {
                    if (thing != this && (thing.def == placement.thing ||
                        (thing.def.entityDefToBuild != null && thing.def.entityDefToBuild == placement.thing)))
                    {
                        isBuilt = true;
                        break;
                    }
                }

                if (!isBuilt)
                {
                    unbuiltPlacements.Add(placement);
                }
            }

            return unbuiltPlacements;
        }
        #endregion

        #region Gizmos and Inspection

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

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

            // Add gizmo to toggle removing last stage on progress
            yield return new Command_Toggle
            {
                defaultLabel = "Remove previous stage",
                defaultDesc = "If enabled, items from the previous stage will be removed when progressing to the next stage.",
                isActive = () => removeLastStageOnProgress,
                toggleAction = () => removeLastStageOnProgress = !removeLastStageOnProgress
            };

            // Add gizmo to manually remove the last stage
            if (lastStageThings.Count > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Remove last stage items",
                    defaultDesc = "Remove all items that were built in the last stage.",
                    action = () => DestroyLastStageThings()
                };
            }
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

                    if (removeLastStageOnProgress)
                    {
                        sb.AppendLine("Previous stage removal: Enabled");
                    }

                    if (lastStageThings.Count > 0)
                    {
                        sb.AppendLine("Last stage items: " + lastStageThings.Count);
                    }
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
            Scribe_Values.Look(ref removeLastStageOnProgress, "removeLastStageOnProgress", false);

            Scribe_Values.Look(ref skipFlags, "skipFlags", GrowingSkipFlags.none);
            Scribe_Values.Look(ref includeNaturalTerrain, "includeNaturalTerrain", false);

            Scribe_Defs.Look(ref overrideWallStuff, "overrideWallStuff");
            Scribe_Defs.Look(ref overrideFloorStuff, "overrideFloorStuff");
            Scribe_Defs.Look(ref overrideDoorStuff, "overrideDoorStuff");
            Scribe_Defs.Look(ref overrideFurnitureStuff, "overrideFurnitureStuff");

            Scribe_Deep.Look(ref ThingFilter, "thingFilter");

            Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Reference);
            Scribe_Collections.Look(ref lastStageThings, "lastStageThings", LookMode.Reference);
        }
    }
}
