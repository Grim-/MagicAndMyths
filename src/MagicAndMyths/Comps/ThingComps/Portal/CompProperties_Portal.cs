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
        public bool oneTimeUse = false;
        public int cooldownTicks = -1;
        public List<ThingDef> requiredFuel = null;
        public int fuelAmountRequired = 0;
        public string displayString = "Enter Portal";

        public ThingDef activePortalEffecter;

        public CompProperties_Portal()
        {
            compClass = typeof(Comp_Portal);
        }
    }

    public class Comp_Portal : ThingComp, IPortalProvider
    {
        protected bool isPortalOpen = false;
        protected int lastUsedTick = -1;
        protected Mote portalEffect = null;

        public CompProperties_Portal Props => (CompProperties_Portal)props;
        private bool CooldownActive => Props.cooldownTicks > 0 &&
                             lastUsedTick > 0 &&
                             (Find.TickManager.TicksGame - lastUsedTick) < Props.cooldownTicks;

        public bool IsPortalActive => isPortalOpen;

        public override void CompTick()
        {
            base.CompTick();

            if (portalEffect != null && isPortalOpen)
            {
                portalEffect.ForceSpawnTick(Find.TickManager.TicksGame);
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
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

            OnDestroyed();

            base.PostDestroy(mode, previousMap);
        }

        protected virtual bool CanOpen() 
        { 
            return true;
        }

        protected virtual void OnOpened() 
        {
            SpawnPortalVisual();
        }

        protected virtual void SpawnPortalVisual()
        {
            if (Props.activePortalEffecter != null && this.portalEffect == null)
            {
                this.portalEffect = MoteMaker.MakeStaticMote(this.parent.Position, this.parent.Map, Props.activePortalEffecter);
            }
        }

        protected virtual void DestroyPortalVisual()
        {
            if (portalEffect != null && !portalEffect.Destroyed)
            {
                portalEffect.Destroy();
                portalEffect = null;
            }

        }

        protected virtual void OnClosed()
        {
        
        }

        protected virtual void OnDestroyed()
        { 
        
        }

        protected virtual bool DoTeleport(Pawn pawn)
        { 
            return false;
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

            if (Props.requiredFuel != null && !HasSufficientFuel())
            {
                Messages.Message("Cannot open portal: Insufficient fuel", MessageTypeDefOf.RejectInput);
                return;
            }

            if (CanOpen())
            {
                isPortalOpen = true;
                lastUsedTick = Find.TickManager.TicksGame;

                if (Props.requiredFuel != null && Props.fuelAmountRequired > 0)
                {
                    ConsumeFuel();
                }

                OnOpened();
                Messages.Message("Portal opened successfully", MessageTypeDefOf.PositiveEvent);
            }
        }

        public void ClosePortal()
        {
            if (!isPortalOpen)
                return;


            isPortalOpen = false;
            DestroyPortalVisual();
            OnClosed();

            Messages.Message("Portal closed", MessageTypeDefOf.NeutralEvent);
        }

        public bool TeleportPawn(Pawn pawn)
        {
            if (!isPortalOpen)
                return false;

            return DoTeleport(pawn);
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
    }
}