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
        //// Structure size
        //public IntVec2 size;

        // List of all building stages
        public List<BuildingStage> stages = new List<BuildingStage>();
    }

    public class BuildingStage
    {
        // Structure size
        public IntVec2 size;
        // All the things to place in this stage
        public List<TerrainPlacement> terrain = new List<TerrainPlacement>();
        public List<ThingPlacement> walls = new List<ThingPlacement>();
        public List<ThingPlacement> doors = new List<ThingPlacement>();
        public List<ThingPlacement> power = new List<ThingPlacement>();
        public List<ThingPlacement> furniture = new List<ThingPlacement>();
        public List<ThingPlacement> other = new List<ThingPlacement>();
    }

    public class TerrainPlacement : ThingPlacement
    {
        public TerrainDef terrain;
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
        public ThingDef rootDef;
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

        private bool includeNaturalTerrain = false;
        private bool includeThingsOnGround = false;

        private ThingDef overrideWallStuff;
        private ThingDef overrideFloorStuff;

        private ThingFilter ThingFilter;
        private List<Thing> placedThings = new List<Thing>();
        private bool showPreview = true;

        private int rootGrowthInterval = 250;
        private int rootGrowthTick = 0;
        private List<Thing> placedRoots = new List<Thing>();
        private int maxRootDistance = 10;

        private int currentStagePreviewIndex = 0;
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


            rootGrowthTick++;
            if (rootGrowthTick >= rootGrowthInterval)
            {
                rootGrowthTick = 0;
                TryGrowRoot();
            }

        }
        private void TryGrowRoot()
        {
            IntVec3 sourcePos = Position;

            placedRoots.RemoveAll(r => r == null || r.Destroyed);

            if (placedRoots.Count > 0)
            {
                sourcePos = placedRoots.Last().Position;
            }

            List<IntVec3> directions = GenAdj.CardinalDirections.ToList();
            directions.Shuffle();

            foreach (IntVec3 offset in directions)
            {
                IntVec3 targetPos = sourcePos + offset;

                // Basic validity checks
                if (!targetPos.InBounds(Map)) 
                    continue;

                if ((targetPos - Position).LengthHorizontalSquared > maxRootDistance * maxRootDistance) 
                    continue;

                bool rootExists = false;
                TerrainDef terrain = Map.terrainGrid.TerrainAt(targetPos);

                if (terrain.passability == Traversability.Impassable) 
                    continue;

                foreach (Thing thing in targetPos.GetThingList(Map))
                {
                    if (thing.def == Def.rootDef)
                    {
                        rootExists = true;
                        break;
                    }
                }

                if (rootExists)
                    continue;

                Thing root = ThingMaker.MakeThing(Def.rootDef);
                Thing placedRoot = GenSpawn.Spawn(root, targetPos, Map);

                if (placedRoot != null)
                {
                    placedRoots.Add(placedRoot);
                    FleckMaker.ThrowDustPuff(targetPos, Map, 0.5f);
                    return;
                }
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

            // Calculate and draw the tree's center
            IntVec2 treeSize = def.size;
            IntVec3 treeCenter = new IntVec3(
                Position.x + treeSize.x / 2,
                Position.y,
                Position.z + treeSize.z / 2
            );

            // Draw the tree's exact position in yellow
            GenDraw.DrawFieldEdges(new List<IntVec3> { Position }, Color.yellow);

            // Draw the tree's center in green
            GenDraw.DrawFieldEdges(new List<IntVec3> { treeCenter }, Color.green);
;
            IntVec3 layoutCenter = new IntVec3(
                Position.x + treeSize.x / 2,
                Position.y,
                Position.z + treeSize.z / 2
            );
            GenDraw.DrawFieldEdges(new List<IntVec3> { layoutCenter }, Color.magenta);

            DrawStagePreview(layout, allPreviewCells);

            if (allPreviewCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(allPreviewCells, Color.cyan);
            }
        }


        private void DrawStagePreview(StructureLayoutDef layout, List<IntVec3> allPreviewCells)
        {
            int previewStage = Mathf.Clamp(currentStagePreviewIndex, 0, layout.stages.Count - 1);
            BuildingStage currentBStage = layout.stages[previewStage];
            DrawThingPreviews(currentBStage.walls, nextWallIndex, allPreviewCells);
            DrawThingPreviews(currentBStage.doors, nextDoorIndex, allPreviewCells);
            DrawThingPreviews(currentBStage.power, nextPowerIndex , allPreviewCells);
            DrawThingPreviews(currentBStage.furniture, nextFurnitureIndex, allPreviewCells);
            DrawThingPreviews(currentBStage.other,  nextOtherIndex, allPreviewCells);
        }

        private void DrawThingPreviews(List<ThingPlacement> things, int startIndex, List<IntVec3> allPreviewCells)
        {
            for (int i = startIndex; i < things.Count; i++)
            {

                if (!CanBuild(things[i]))
                {
                    continue;
                }

                ThingPlacement placement = things[i];
                IntVec3 pos = CalculateCenteredPosition(placement.position);

                if (pos.InBounds(Map) && !IsCellOccupiedByThisBuilding(pos))
                {
                    allPreviewCells.Add(pos);

                    // Draw ghost preview
                    Vector3 drawPos = pos.ToVector3Shifted();
                    drawPos.y = AltitudeLayer.Blueprint.AltitudeFor();

                    // Get ghost graphic
                    Graphic ghostGraphic = GhostUtility.GhostGraphicFor(placement.thing.graphic, placement.thing, Color.cyan, placement.stuff);
                    if (ghostGraphic != null)
                    {
                        Mesh mesh = ghostGraphic.MeshAt(placement.rotation);
                        Material mat = ghostGraphic.MatAt(placement.rotation, null);
                        Quaternion quat = placement.rotation.AsQuat;
                        drawPos += ghostGraphic.DrawOffset(placement.rotation);
                        Graphics.DrawMesh(mesh, drawPos, quat, mat, 0);
                    }
                }
            }
        }


        private bool CanBuild(ThingPlacement thingPlacement)
        {
            return thingPlacement.thing != null && thingPlacement.thing.BuildableByPlayer;
        }

        private void GrowTick()
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return;
            StructureLayoutDef layout = growableDef.structureLayout;

            int stageCount = layout.stages.Count;
            if (stageCount <= 0)
                return;


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
 
            if (IsGrown())
            {
                FinishGrowth();
            }
        }



        private bool IsGrown()
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

            // Build in a specific order: terrain -> walls -> doors -> power -> furniture -> other
            if (nextTerrainIndex < stage.terrain.Count)
            {
                BuildTerrain(stage.terrain[nextTerrainIndex]);
                nextTerrainIndex++;
                return true;
            }

            if (nextWallIndex < stage.walls.Count)
            {
                BuildThing(stage.walls[nextWallIndex]);
                nextWallIndex++;
                return true;
            }


            if (nextDoorIndex < stage.doors.Count)
            {
                BuildThing(stage.doors[nextDoorIndex]);
                nextDoorIndex++;
                return true;
            }


            if (nextPowerIndex < stage.power.Count)
            {
                BuildThing(stage.power[nextPowerIndex]);
                nextPowerIndex++;
                return true;
            }


            if (nextFurnitureIndex < stage.furniture.Count)
            {
                BuildThing(stage.furniture[nextFurnitureIndex]);
                nextFurnitureIndex++;
                return true;
            }

            if (nextOtherIndex < stage.other.Count)
            {
                BuildThing(stage.other[nextOtherIndex]);
                nextOtherIndex++;
                return true;
            }

            return false;
        }

        private void BuildTerrain(TerrainPlacement terrain)
        {
            if (terrain.terrain == null)
                return;

            if (!CanBuild(terrain))
            {
                return;
            }
            IntVec3 pos = CalculateCenteredPosition(terrain.position);

            // Ensure it's within bounds
            if (!pos.InBounds(Map))
                return;

            if (IsCellOccupiedByThisBuilding(pos))
                return;

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

            if (IsCellOccupiedByThisBuilding(pos) || !CanBuild(placement))
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
                return Position;

            int treeCenterX = Position.x;
            int treeCenterZ = Position.z;

            return new IntVec3(
                treeCenterX + (relativePos.x),
                Position.y,
                treeCenterZ + (relativePos.z)
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
            //this.Destroy();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode == DestroyMode.KillFinalize)
            {
                DestroyPlacedThings();

                DestroyRoots();
            }

            base.Destroy(mode);
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
        private void DestroyRoots()
        {
            foreach (Thing root in placedRoots)
            {
                if (root != null && !root.Destroyed)
                {
                    root.Destroy();
                }
            }

            placedRoots.Clear();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }

            if (Prefs.DevMode)
            {

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
                                options.Add(new FloatMenuOption($"Stage {i}", () =>
                                {
                                    SetPreviewIndex(i);
                                }));
                            }
                        }

                        Find.WindowStack.Add(new FloatMenu(options));
                    }
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

            Scribe_Values.Look(ref rootGrowthTick, "rootGrowthTick", 0);
            Scribe_Collections.Look(ref placedRoots, "placedRoots", LookMode.Reference);
        }
    }
}
