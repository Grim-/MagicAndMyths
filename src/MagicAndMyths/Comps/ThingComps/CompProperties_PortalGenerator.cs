using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PortalGenerator : CompProperties
    {
        public MapGeneratorDef mapGeneratorDef;
        public IntVec3 mapSize = new IntVec3(75, 1, 75);

        public CompProperties_PortalGenerator()
        {
            this.compClass = typeof(Comp_PortalGenerator);
        }
    }

    public class Comp_PortalGenerator : ThingComp
    {

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

        public CompProperties_PortalGenerator Props => (CompProperties_PortalGenerator)props;
        public void CreatePortal()
        {
            if (Props.mapGeneratorDef == null)
            {
                Messages.Message("Cannot create portal: Missing map generator", MessageTypeDefOf.RejectInput);
                return;
            }

            IntVec3 portalPos = GenAdj.CellsAdjacent8Way(this.parent).FirstOrDefault(c => c.Standable(this.parent.Map));

            if (!portalPos.Standable(this.parent.Map))
            {
                Messages.Message("Cannot create portal: No space available", MessageTypeDefOf.RejectInput);
                return;
            }

            int newPortalID = PortalUtils.NewPortalID(parent);
            Map linkedMap = DungeonManager.GetOrCreateDungeonMap(
               newPortalID,
               this.parent.Map,
                Props.mapGeneratorDef,
                Props.mapSize,
                this.parent.Map.Tile
            );
            if (linkedMap == null)
            {
                Messages.Message("Failed to create destination map", MessageTypeDefOf.RejectInput);
                return;
            }

            Portal portal = PortalUtils.CreatePortal(linkedMap, newPortalID);
            if (portal != null)
            {
                GenSpawn.Spawn(portal, portalPos, this.parent.Map);
                Messages.Message("Portal created successfully", MessageTypeDefOf.PositiveEvent);
                return;
            }
            else
            {
                Messages.Message("Failed to create portal", MessageTypeDefOf.RejectInput);
                return;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Create portal command
            yield return new Command_Action
            {
                defaultLabel = "Create Portal",
                defaultDesc = "Create a portal to a new map.",
                icon = TexButton.Play,
                action = CreatePortal
            };
        }
    }
}