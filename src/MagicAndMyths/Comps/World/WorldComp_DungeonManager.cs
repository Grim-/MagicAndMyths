using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class WorldComp_DungeonManager : WorldComponent
    {
        protected Dictionary<int, DungeonMapParent> DungeonMaps = new Dictionary<int, DungeonMapParent>();
        public static Map StartingColonyMap => Find.Maps.First(x => x.IsPlayerHome);

        public WorldComp_DungeonManager(World world) : base(world)
        {

        }

        public Map GetOrCreateDungeonMap(int uniqueId, Map originMap, MapGeneratorDef mapGeneratorDef, IntVec3 mapSize, int sourceTile)
        {
            if (DungeonMaps.TryGetValue(uniqueId, out DungeonMapParent existingParent))
            {
                if (existingParent.HasMap)
                    return existingParent.Map;

                Map map = MapGenerator.GenerateMap(
                    mapSize,
                    existingParent,
                    mapGeneratorDef,
                    null,
                    null,
                    true
                );
                return map;
            }

  
            return CreateNewDungeoMap(uniqueId, originMap, mapGeneratorDef, mapSize, sourceTile);
        }


        private Map CreateNewDungeoMap(int uniqueId, Map originMap, MapGeneratorDef mapGeneratorDef, IntVec3 mapSize, int sourceTile)
        {
            DungeonMapParent mapParent = (DungeonMapParent)WorldObjectMaker.MakeWorldObject(MagicAndMythDefOf.DungeonMapParent);
            mapParent.SetDungeonID(uniqueId);
            mapParent.SetOriginMap(originMap);
            mapParent.Tile = sourceTile;
            Find.WorldObjects.Add(mapParent);

            Map customMap = MapGenerator.GenerateMap(
                mapSize,
                mapParent,
                mapGeneratorDef,
                null,
                null,
                true
            );
            DungeonMaps[uniqueId] = mapParent;
            return customMap;
        }

        public Map GetMapWithID(int uniqueID)
        {
            if (HasMapWithID(uniqueID))
            {
                if (DungeonMaps[uniqueID].HasMap)
                {
                    return DungeonMaps[uniqueID].Map;
                }
            }

            return null;
        }

        public bool HasMapWithID(int uniqueID)
        {
            return DungeonMaps.ContainsKey(uniqueID);
        }


        public bool TryCloseMap(int uniqueID)
        {
            if (HasMapWithID(uniqueID))
            {       
                Map dungeonMap = GetMapWithID(uniqueID);
                DungeonMapParent mapParent = dungeonMap.Parent as DungeonMapParent;

                if (dungeonMap != null)
                {
                    if (mapParent != null)
                    {
                        mapParent.CloseMap();
                    }

                    Current.Game.DeinitAndRemoveMap(dungeonMap, true);
                    DungeonMaps.Remove(uniqueID);
                    return true;
                }
            }
            return false;
        }

        public bool TryCloseMap(Map map)
        {
            if (map.Parent is DungeonMapParent dungeonParent)
            {
                return TryCloseMap(dungeonParent.DungeonID);
            }

            return false;
        }

        public bool TryGetMapWithID(int uniqueID, out DungeonMapParent dungeonMap)
        {
            dungeonMap = null;
            if (HasMapWithID(uniqueID))
            {
                dungeonMap = DungeonMaps[uniqueID];
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref DungeonMaps, "dungeonMaps", LookMode.Value, LookMode.Reference);
        }
    }
}
