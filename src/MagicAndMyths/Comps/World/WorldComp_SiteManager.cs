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

                        return map;
                    }
                    else if (storedData.mapParent?.HasMap == true)
                    {
                        Log.Message($"Found stored active map for tile {tileId}");
                        return storedData.mapParent.Map;
                    }
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


        public bool HasStoredDataForTileID(int tileId)
        {
            return storedSites.Any(s => s.tileId == tileId);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storedSites, "storedSites", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (storedSites == null)
                {
                    storedSites = new List<StoredSiteData>();
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
