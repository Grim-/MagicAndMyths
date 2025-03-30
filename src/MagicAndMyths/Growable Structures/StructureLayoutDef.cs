using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class StructureLayoutDef : Def
    {
        public List<BuildingStage> stages = new List<BuildingStage>();
        public BuildingStage GetStage(int index)
        {
            if (index < 0 || index > stages.Count)
            {
                return null;
            }

            return stages[index];
        }

        //Used by the editor, purely for data modelling, user is expected to export Def to xml.
        public StructureLayoutDef DeepCopy()
        {
            StructureLayoutDef copiedDef = new StructureLayoutDef();
            copiedDef.defName = this.defName;
            copiedDef.stages = new List<BuildingStage>();

            foreach (var stage in this.stages)
            {
                var newStage = new BuildingStage
                {
                    size = stage.size,
                    terrain = new List<TerrainPlacement>(
                        stage.terrain.Select(t => new TerrainPlacement
                        {
                            terrain = t.terrain,
                            position = t.position
                        })
                    ),
                    walls = new List<ThingPlacement>(
                        stage.walls.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    doors = new List<ThingPlacement>(
                        stage.doors.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    power = new List<ThingPlacement>(
                        stage.power.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    furniture = new List<ThingPlacement>(
                        stage.furniture.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    ),
                    other = new List<ThingPlacement>(
                        stage.other.Select(t => new ThingPlacement
                        {
                            thing = t.thing,
                            stuff = t.stuff,
                            position = t.position,
                            rotation = t.rotation
                        })
                    )
                };
                copiedDef.stages.Add(newStage);
            }
            return copiedDef;
        }
    }

    public class BuildingStage
    {
        public IntVec2 size;
        public bool destroyPreviousStage = false;
        public List<TerrainPlacement> terrain = new List<TerrainPlacement>();
        public List<ThingPlacement> walls = new List<ThingPlacement>();
        public List<ThingPlacement> doors = new List<ThingPlacement>();
        public List<ThingPlacement> power = new List<ThingPlacement>();
        public List<ThingPlacement> furniture = new List<ThingPlacement>();
        public List<ThingPlacement> other = new List<ThingPlacement>();

        public Dictionary<ThingDef, int> GetRequiredResources()
        {
            Dictionary<ThingDef, int> resources = new Dictionary<ThingDef, int>();
            foreach (TerrainPlacement placement in terrain)
            {
                if (placement.terrain == null || placement.terrain.costList == null)
                    continue;

                foreach (ThingDefCountClass cost in placement.terrain.costList)
                {
                    AddResource(resources, cost.thingDef, cost.count);
                }
            }
            ProcessPlacements(resources, walls);
            ProcessPlacements(resources, doors);
            ProcessPlacements(resources, power);
            ProcessPlacements(resources, furniture);
            ProcessPlacements(resources, other);

            return resources;
        }

        private void ProcessPlacements(Dictionary<ThingDef, int> resources, List<ThingPlacement> placements)
        {
            foreach (ThingPlacement placement in placements)
            {
                if (placement.thing == null)
                    continue;

                if (placement.thing.costList != null)
                {
                    foreach (ThingDefCountClass cost in placement.thing.costList)
                    {
                        AddResource(resources, cost.thingDef, cost.count);
                    }
                }

                if (placement.stuff != null && placement.thing.costStuffCount > 0)
                {
                    AddResource(resources, placement.stuff, placement.thing.costStuffCount);
                }
            }
        }

        private void AddResource(Dictionary<ThingDef, int> resources, ThingDef thing, int count)
        {
            if (thing == null)
                return;

            if (resources.ContainsKey(thing))
            {
                resources[thing] += count;
            }
            else
            {
                resources[thing] = count;
            }
        }
    }


    public class TerrainPlacement : ThingPlacement
    {
        public TerrainDef terrain;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref terrain, "terrain");
        }
    }

    public class ThingPlacement : IExposable
    {
        public ThingDef thing;
        public IntVec2 position;
        public Rot4 rotation = Rot4.North;
        public ThingDef stuff;

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref thing, "thing");
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref rotation, "rotation");
            Scribe_Defs.Look(ref stuff, "stuff");
        }
       
    }
}
