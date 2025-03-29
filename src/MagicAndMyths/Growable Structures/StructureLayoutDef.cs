using System.Collections.Generic;
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
    }

    public class ThingPlacement
    {
        public ThingDef thing;
        public IntVec2 position;
        public Rot4 rotation = Rot4.North;
        public ThingDef stuff;
    }
}
