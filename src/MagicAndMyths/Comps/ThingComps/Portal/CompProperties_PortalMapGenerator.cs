using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PortalMapGenerator : CompProperties_Portal
    {
        public MapGeneratorDef mapGeneratorDef;
        public IntVec3 mapSize = new IntVec3(75, 1, 75);

        public CompProperties_PortalMapGenerator()
        {
            this.compClass = typeof(Comp_PortalMapGenerator);
        }
    }

    public class Comp_PortalMapGenerator : Comp_Portal
    {
        private Map linkedMap = null;
        private int uniqueMapId = -1;

        private WorldComp_DungeonManager _dungeonManager;
        private WorldComp_DungeonManager DungeonManager
        {
            get
            {
                if (_dungeonManager == null)
                {
                    _dungeonManager = Find.World.GetComponent<WorldComp_DungeonManager>();
                }
                return _dungeonManager;
            }
        }

        public CompProperties_PortalMapGenerator MapGenProps => (CompProperties_PortalMapGenerator)props;
        public Map LinkedMap => linkedMap;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                if (uniqueMapId == -1)
                {
                    uniqueMapId = Find.TickManager.TicksGame + this.parent.thingIDNumber;
                }
            }
        }

        protected override bool CanOpen()
        {
            if (MapGenProps.mapGeneratorDef == null)
            {
                return false;
            }

            return true;
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            linkedMap = GetOrCreatePortalMap();

            if (linkedMap == null)
            {
                Messages.Message("Failed to create destination map", MessageTypeDefOf.RejectInput);
                isPortalOpen = false;
                return;
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            linkedMap = null;

            if (DungeonManager != null)
            {
                DungeonManager.TryCloseMap(uniqueMapId);
            }
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            if (DungeonManager != null && DungeonManager.TryGetMapWithID(uniqueMapId, out DungeonMapParent dungeonMapParent))
            {
                DungeonManager.TryCloseMap(uniqueMapId);
            }
        }

        protected override bool DoTeleport(Pawn pawn)
        {
            if (linkedMap == null)
                return false;

            if (DungeonManager.TryGetMapWithID(uniqueMapId, out DungeonMapParent dungeonMapParent))
            {
                dungeonMapParent.MoveToMap(pawn);
                return true;
            }

            IntVec3 spawnLoc = PortalUtils.FindTeleportLocation(pawn, linkedMap);
            if (!spawnLoc.IsValid)
                return false;

            pawn.DeSpawn(DestroyMode.Vanish);
            GenSpawn.Spawn(pawn, spawnLoc, linkedMap);

            return true;
        }

        private Map GetOrCreatePortalMap()
        {
            return DungeonManager.GetOrCreateDungeonMap(
               uniqueMapId,
               this.parent.Map,
                MapGenProps.mapGeneratorDef,
                MapGenProps.mapSize,
                this.parent.Map.Tile
            );
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref isPortalOpen, "isPortalOpen", false);
            Scribe_References.Look(ref linkedMap, "linkedMap");
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", -1);
            Scribe_Values.Look(ref uniqueMapId, "uniqueMapId", -1);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (isPortalOpen && uniqueMapId != -1)
                {
                    linkedMap = GetOrCreatePortalMap();
                }
            }
        }
    }
}