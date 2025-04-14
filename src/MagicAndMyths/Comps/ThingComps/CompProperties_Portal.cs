using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class CompProperties_Portal : CompProperties
    {
        public MapGeneratorDef mapGeneratorDef;
        public IntVec3 mapSize = new IntVec3(75, 1, 75);
        public bool oneTimeUse = false;
        public int cooldownTicks = -1;
        public List<ThingDef> requiredFuel = null;
        public int fuelAmountRequired = 0;

        public string displayString = "Enter Portal";

        public CompProperties_Portal()
        {
            this.compClass = typeof(Comp_Portal);
        }
    }

    /// <summary>
    /// Turns the thing into a portal
    /// </summary>
    public class Comp_Portal : ThingComp, IPortalProvider
    {
        private bool isPortalOpen = false;
        private Map linkedMap = null;
        private int lastUsedTick = -1;
        private Effecter portalEffect = null;
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

        public CompProperties_Portal Props => (CompProperties_Portal)props;
        private bool CooldownActive => Props.cooldownTicks > 0 &&
                             lastUsedTick > 0 &&
                             (Find.TickManager.TicksGame - lastUsedTick) < Props.cooldownTicks;

        public bool IsPortalActive => isPortalOpen;
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

        public override void CompTick()
        {
            base.CompTick();

            if (portalEffect != null && isPortalOpen)
            {
                portalEffect.EffectTick(this.parent, this.parent);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (isPortalOpen)
            {
                ClosePortal();
            }

            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (isPortalOpen)
            {
                ClosePortal();
            }

            if (DungeonManager != null && DungeonManager.TryGetMapWithID(uniqueMapId, out DungeonMapParent dungeonMapParent))
            {
                DungeonManager.TryCloseMap(uniqueMapId);
            }

            base.PostDestroy(mode, previousMap);
        }

        public void OpenPortal()
        {
            if (isPortalOpen)
                return;

            if (CooldownActive)
            {
                Messages.Message("Cannot open portal: Cooldown active", MessageTypeDefOf.RejectInput);
                return;
            }

            if (Props.mapGeneratorDef == null)
            {
                return;
            }

            if (Props.requiredFuel != null && !HasSufficientFuel())
            {
                Messages.Message("Cannot open portal: Insufficient fuel", MessageTypeDefOf.RejectInput);
                return;
            }

            linkedMap = GetOrCreatePortalMap();

            if (linkedMap == null)
            {
                Messages.Message("Failed to create destination map", MessageTypeDefOf.RejectInput);
                return;
            }

            isPortalOpen = true;
            lastUsedTick = Find.TickManager.TicksGame;

            // Consume fuel if required
            if (Props.requiredFuel != null && Props.fuelAmountRequired > 0)
            {
                ConsumeFuel();
            }

            Messages.Message("Portal opened successfully", MessageTypeDefOf.PositiveEvent);
        }

        public void ClosePortal()
        {
            if (!isPortalOpen)
                return;

            // Clean up portal effect
            if (portalEffect != null)
            {
                portalEffect.Cleanup();
                portalEffect = null;
            }

            isPortalOpen = false;
            linkedMap = null;

            if (DungeonManager != null)
            {
                DungeonManager.TryCloseMap(uniqueMapId);
            }

            Messages.Message("Portal closed", MessageTypeDefOf.NeutralEvent);
        }

        private Map GetOrCreatePortalMap()
        {
            return DungeonManager.GetOrCreateDungeonMap(
               uniqueMapId,
               this.parent.Map,
                Props.mapGeneratorDef,
                Props.mapSize,
                this.parent.Map.Tile
            );
        }

        public bool TeleportPawn(Pawn pawn)
        {
            if (!isPortalOpen || linkedMap == null)
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

        private bool HasSufficientFuel()
        {
            if (Props.requiredFuel == null || Props.fuelAmountRequired <= 0)
                return true;

            int totalAvailable = 0;
            foreach (ThingDef fuelDef in Props.requiredFuel)
            {
                totalAvailable += this.parent.Map.listerThings.ThingsOfDef(fuelDef)
                    .Where(t => t.Position.InHorDistOf(this.parent.Position, 5f))
                    .Sum(t => t.stackCount);
            }

            return totalAvailable >= Props.fuelAmountRequired;
        }
        private void ConsumeFuel()
        {
            if (Props.requiredFuel == null || Props.fuelAmountRequired <= 0)
                return;

            int remaining = Props.fuelAmountRequired;

            foreach (ThingDef fuelDef in Props.requiredFuel)
            {
                List<Thing> availableFuel = this.parent.Map.listerThings.ThingsOfDef(fuelDef)
                    .Where(t => t.Position.InHorDistOf(this.parent.Position, 5f))
                    .ToList();

                foreach (Thing fuel in availableFuel)
                {
                    int toConsume = Mathf.Min(remaining, fuel.stackCount);
                    fuel.SplitOff(toConsume).Destroy();
                    remaining -= toConsume;

                    if (remaining <= 0)
                        break;
                }

                if (remaining <= 0)
                    break;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!isPortalOpen)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Open Portal",
                    defaultDesc = "Open a portal to the destination map.",
                    icon = TexButton.Play,
                    action = OpenPortal,
                    Disabled = CooldownActive,
                    disabledReason = CooldownActive ? "Portal on cooldown" : null
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Close Portal",
                    defaultDesc = "Close the active portal.",
                    icon = TexCommand.ForbidOn,
                    action = ClosePortal
                };
            }
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (IsPortalActive)
            {
                yield return new FloatMenuOption(Props.displayString, () =>
                {
                    Job job = JobMaker.MakeJob(MagicAndMythDefOf.Portals_UsePortalJob, this.parent);
                    selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                });
            }
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