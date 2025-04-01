using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class Building_ReturnPortal : Building, IPortalProvider
    {
        private PortalMode mode = PortalMode.Single;
        private float groupRadius = 4f;
        private Map homeMap;
        protected Map ReturnMap => homeMap != null ? homeMap : WorldCustomSiteManager.StartingColonyMap;

        private Thing PortalThingOrigin = null;

        public bool IsPortalActive => true;

        private Effecter portalEffect;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (portalEffect != null)
            {
                portalEffect.Cleanup();
                portalEffect = null;
            }
            base.DeSpawn(mode);
        }


        public void SetHomeMap(Map map)
        {
            homeMap = map;
        }

        public void SetPortalThingOrigin(Thing thing)
        {
            PortalThingOrigin = thing;
        }

        public override void Tick()
        {
            base.Tick();
            if (portalEffect != null)
            {
                portalEffect.ticksLeft = 999;
                portalEffect.EffectTick(this, this);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn?.Faction != Faction.OfPlayer)
                yield break;

            yield return new FloatMenuOption("Use Portal", () =>
            {
                if (mode == PortalMode.Single)
                {
                    Job job = JobMaker.MakeJob(MagicAndMythDefOf.Portals_UsePortalJob, this);
                    selPawn.jobs.StartJob(job);
                }
                else
                {
                    TeleportGroup(selPawn);
                }
            });
        }

        public bool TeleportPawn(Pawn pawn)
        {
            if (ReturnMap == null || pawn == null)
                return false;

           return pawn.TransferToMap(CellFinderLoose.TryFindCentralCell(ReturnMap, 10, 1), ReturnMap);
        }

        private void TeleportGroup(Pawn initiator)
        {
            if (initiator?.Map == null)
                return;

            var pawnsInRadius = GenRadial.RadialDistinctThingsAround(initiator.Position, initiator.Map, groupRadius, true)
                .OfType<Pawn>()
                .Where(p => p.Faction == initiator.Faction && !p.Dead && !p.Downed)
                .ToList();

            int successCount = 0;
            foreach (var pawn in pawnsInRadius)
            {
                if (TeleportPawn(pawn))
                {
                    successCount++;
                }
            }

            if (successCount > 0)
            {
                Messages.Message($"Teleported {successCount} colonists", MessageTypeDefOf.PositiveEvent);
            }
        }

        private bool TeleportLocationValidator(Pawn pawn, Map map, IntVec3 position)
        {
            return position.Standable(map) && !position.Filled(map) && position.GetDangerFor(pawn, map) != Danger.Deadly;
        }

        public void SetRadius(float radius)
        {
            groupRadius = radius;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mode, "mode", PortalMode.Single);
            Scribe_Values.Look(ref groupRadius, "groupRadius", 4f);
        }
    }
}
