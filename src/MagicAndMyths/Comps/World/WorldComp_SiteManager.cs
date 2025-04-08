using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class WorldComp_SiteManager : WorldComponent
    {
        private List<StoredSiteData> storedSites = new List<StoredSiteData>();
        // Separate list for component portal data
        private List<ComponentPortalData> componentPortals = new List<ComponentPortalData>();



        public static Map StartingColonyMap => Find.Maps.First(x => x.IsPlayerHome);


        public List<SitePartDef> AvailableSiteParts = new List<SitePartDef>()
        {
            SitePartDefOf.PreciousLump,
            SitePartDefOf.BanditCamp,
            SitePartDefOf.AncientComplex,
            SitePartDefOf.AncientAltar,
        };

        public WorldComp_SiteManager(World world) : base(world)
        {

        }

        public Map GetOrCreateMapForTile(int tileId, MapGeneratorDef mapGenDef, List<SitePartDef> sitePartDefs = null)
        {
            if (HasStoredDataForTileID(tileId))
            {
                var storedData = storedSites.FirstOrDefault(s => s.tileId == tileId);
                if (storedData != null)
                {
                    Log.Message($"Getting map for tile {tileId}");

                    if (storedData.mapParent != null && !storedData.mapParent.HasMap)
                    {
                        Log.Message($"Generation stored map for tile {tileId}");

                        if (storedData.site != null && !Find.WorldObjects.Contains(storedData.site))
                        {
                            Find.WorldObjects.Add(storedData.site);
                        }

                        if (!Find.WorldObjects.Contains(storedData.mapParent))
                        {
                            Find.WorldObjects.Add(storedData.mapParent);
                        }

                        Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tileId, Find.World.info.initialMapSize, null);

                        storedData.mapParent.AddModifier(new MapModifier_RandomFires(map));
                        return map;
                    }
                    else if (storedData.mapParent?.HasMap == true)
                    {
                        Log.Message($"Found stored active map for tile {tileId}");
                        return storedData.mapParent.Map;
                    }
                }
            }

            // Check for component portal with this ID
            var componentPortal = componentPortals.FirstOrDefault(p => p.uniqueId == tileId);
            if (componentPortal != null)
            {
                if (componentPortal.mapParent != null && !componentPortal.mapParent.HasMap)
                {
                    Log.Message($"Generating stored component portal map for ID {tileId}");

                    if (!Find.WorldObjects.Contains(componentPortal.mapParent))
                    {
                        Find.WorldObjects.Add(componentPortal.mapParent);
                    }

                    Map map = MapGenerator.GenerateMap(
                        Find.World.info.initialMapSize,
                        componentPortal.mapParent,
                        componentPortal.mapGeneratorDef,
                        null,
                        null,
                        true);

                    return map;
                }
                else if (componentPortal.mapParent?.HasMap == true)
                {
                    Log.Message($"Found stored active component portal map for ID {tileId}");
                    return componentPortal.mapParent.Map;
                }
            }

            Map existingMap = Current.Game.Maps.Find(x => x.Tile == tileId);
            if (existingMap != null)
            {
                Log.Message($"Found active map for tile {tileId}");
                return existingMap;
            }

            if (Find.WorldObjects.AllWorldObjects.Any(wo => wo.Tile == tileId) || Find.WorldObjects.AnyMapParentAt(tileId))
            {
                Log.Warning($"Attempted to create a site on tile {tileId} which already has a world object");
                return null;
            }

            Map newMap = CreateNewMap(tileId, mapGenDef, sitePartDefs);
            return newMap;
        }

        private Map CreateNewMap(int tileId, MapGeneratorDef mapGenDef, List<SitePartDef> sitePartDefs = null)
        {
            Log.Message($"Creating new map for tile {tileId}");

            if (Find.WorldObjects.AllWorldObjects.Any(wo => wo.Tile == tileId))
            {
                Log.Warning($"Attempted to create a site on tile {tileId} which already has a world object");
                return null;
            }

            DungeonMapParent mapParent = (DungeonMapParent)WorldObjectMaker.MakeWorldObject(MagicAndMythDefOf.DungeonMapParent);
            mapParent.Tile = tileId;

            Find.WorldObjects.Add(mapParent);

            var sitePart = (sitePartDefs ?? AvailableSiteParts).RandomElement();
            Site newSite = SiteMaker.MakeSite(sitePart, tileId, Faction.OfPirates);
            if (newSite != null)
            {
                if (!Find.WorldObjects.Contains(newSite))
                    Find.WorldObjects.Add(newSite);

                Map newMap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, mapParent, mapGenDef, null, null, true);

                mapParent.AddModifier(new MapModifier_RandomFires(newMap));

                newMap.info.parent = mapParent;
                storedSites.Add(new StoredSiteData(tileId, newSite, mapParent));
                return newMap;
            }
            return null;
        }

        // Method for component-based portals
        public Map GetOrCreateComponentPortalMap(int uniqueId, MapGeneratorDef mapGeneratorDef, IntVec3 mapSize, int sourceTile, WorldObjectDef worldObjectDef = null)
        {
            // Check if portal already exists
            var existingPortal = componentPortals.FirstOrDefault(p => p.uniqueId == uniqueId);
            if (existingPortal != null)
            {
                if (existingPortal.mapParent?.HasMap == true)
                {
                    return existingPortal.mapParent.Map;
                }
                else if (existingPortal.mapParent != null)
                {
                    if (!Find.WorldObjects.Contains(existingPortal.mapParent))
                    {
                        Find.WorldObjects.Add(existingPortal.mapParent);
                    }

                    Map map = MapGenerator.GenerateMap(
                        mapSize,
                        existingPortal.mapParent,
                        existingPortal.mapGeneratorDef ?? mapGeneratorDef,
                        null,
                        null,
                        true);

                    return map;
                }
            }

            // Create new portal map
            DungeonMapParent mapParent = (DungeonMapParent)WorldObjectMaker.MakeWorldObject(worldObjectDef != null ? worldObjectDef : MagicAndMythDefOf.DungeonMapParent);

            // Use source tile to avoid cluttering world map
            mapParent.Tile = sourceTile;
            Find.WorldObjects.Add(mapParent);



            Log.Message($"Generating map with MapGenDef {mapGeneratorDef.defName}");

            Map customMap = MapGenerator.GenerateMap(
                mapSize,
                mapParent,
                mapGeneratorDef,
                null,
                null,
                true
            );

            // Store the component portal data
            ComponentPortalData portalData = new ComponentPortalData(uniqueId, mapGeneratorDef, mapParent);
            componentPortals.Add(portalData);

            return customMap;
        }

        public void RemoveStoredDataForTileID(int tileId)
        {
            var storedData = storedSites.FirstOrDefault(s => s.tileId == tileId);
            if (storedData != null)
            {
                if (storedData.site != null && Find.WorldObjects.Contains(storedData.site))
                    Find.WorldObjects.Remove(storedData.site);

                if (storedData.mapParent != null && Find.WorldObjects.Contains(storedData.mapParent))
                {
                    if (storedData.mapParent.HasMap)
                    {
                        Current.Game.DeinitAndRemoveMap(storedData.mapParent.Map, true);
                    }
                    Find.WorldObjects.Remove(storedData.mapParent);
                }

                storedSites.Remove(storedData);
            }
        }

        public void RemoveComponentPortalData(int uniqueId)
        {
            var portalData = componentPortals.FirstOrDefault(p => p.uniqueId == uniqueId);
            if (portalData != null)
            {
                if (portalData.mapParent != null && Find.WorldObjects.Contains(portalData.mapParent))
                {
                    if (portalData.mapParent.HasMap)
                    {
                        EjectColonistAndItemsFromMap(portalData.mapParent.Map);


                        Current.Game.DeinitAndRemoveMap(portalData.mapParent.Map, true);
                    }
                    Find.WorldObjects.Remove(portalData.mapParent);
                }

                componentPortals.Remove(portalData);
            }
        }


        public void EjectColonistAndItemsFromMap(Map map)
        {
            Log.Message("Returning all pawns to a player map");
            foreach (var item in map.mapPawns.AllPawns.ToList())
            {
                if (item.Faction == RimWorld.Faction.OfPlayer)
                {
                    item.TransferToMap(WorldComp_SiteManager.StartingColonyMap.Center, WorldComp_SiteManager.StartingColonyMap);
                }
            }
        }

        public bool HasStoredDataForTileID(int tileId)
        {
            return storedSites.Any(s => s.tileId == tileId);
        }

        public bool HasComponentPortalData(int uniqueId)
        {
            return componentPortals.Any(p => p.uniqueId == uniqueId);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storedSites, "storedSites", LookMode.Deep);
            Scribe_Collections.Look(ref componentPortals, "componentPortals", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (storedSites == null)
                {
                    storedSites = new List<StoredSiteData>();
                }

                if (componentPortals == null)
                {
                    componentPortals = new List<ComponentPortalData>();
                }

                foreach (var storedData in storedSites)
                {
                    if (storedData.site != null && !Find.WorldObjects.Contains(storedData.site))
                    {
                        Log.Message($"Restoring site {storedData.site.ID} to WorldObjects");
                        Find.WorldObjects.Add(storedData.site);
                    }
                }
            }
        }
    }
}
