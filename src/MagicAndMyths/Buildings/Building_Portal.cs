using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class Portal : Building, IPortalProvider
    {
        public Map DestinationMap;
        public int LinkedMapId = -1;
        private Effecter portalEffect = null;
        protected bool IsPortalActive = true;

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

        bool IPortalProvider.IsPortalActive => IsPortalActive;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public void SetPortalActiveStatus(bool newState)
        {
            this.IsPortalActive = newState;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Close();
            base.DeSpawn(mode);
        }

        public void Close()
        {
            if (portalEffect != null)
            {
                portalEffect.Cleanup();
                portalEffect = null;
            }

            if (DungeonManager != null && DestinationMap != null && LinkedMapId != -1)
            {
                if (DungeonManager.TryCloseMap(LinkedMapId))
                {    
                    DestinationMap = null;
                }
            }

            IsPortalActive = false;
            Messages.Message("Portal closed", MessageTypeDefOf.NeutralEvent);
        }

        public override void Tick()
        {
            base.Tick();

            if (portalEffect != null)
            {
                portalEffect.EffectTick(this, this);
            }
        }

        public virtual bool TeleportPawn(Pawn pawn)
        {
            if (DestinationMap == null)
                return false;

            if (LinkedMapId != -1 && DungeonManager.TryGetMapWithID(LinkedMapId, out DungeonMapParent dungeonMapParent))
            {
                dungeonMapParent.MoveToMap(pawn);
                return true;
            }

            IntVec3 spawnLoc = PortalUtils.FindTeleportLocation(pawn, DestinationMap);
            if (!spawnLoc.IsValid)
                return false;

            pawn.DeSpawn(DestroyMode.Vanish);
            GenSpawn.Spawn(pawn, spawnLoc, DestinationMap);
            return true;
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (IsPortalActive)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Close Portal",
                    defaultDesc = "Close the portal",
                    action = () =>
                    {
                        this.Destroy();
                    }
                };
            }

        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (IsPortalActive)
            {
                yield return new FloatMenuOption("Enter Portal", () =>
                {
                    Job job = JobMaker.MakeJob(MagicAndMythDefOf.Portals_UsePortalJob, this);
                    selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                });
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref DestinationMap, "destMap");
            Scribe_Values.Look(ref LinkedMapId, "linkedMapId", -1);
            Scribe_Values.Look(ref IsPortalActive, "IsPortalActive");
        }
    }
}