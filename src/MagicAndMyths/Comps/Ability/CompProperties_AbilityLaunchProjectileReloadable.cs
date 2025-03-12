using RimWorld;
using RimWorld.Utility;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{

    public class CompProperties_AbilityLaunchProjectileReloadable : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityLaunchProjectileReloadable()
        {
            compClass = typeof(CompAbilityEffect_LaunchProjectileReloadable);
        }
    }

    public class CompAbilityEffect_LaunchProjectileReloadable : CompAbilityEffect, IReloadableComp
    {
        private ThingDef LoadedProjectileDef = null;

        public new CompProperties_AbilityLaunchProjectileReloadable Props
        {
            get
            {
                return (CompProperties_AbilityLaunchProjectileReloadable)this.props;
            }
        }

        public Thing ReloadableThing => this.parent.pawn;

        public ThingDef AmmoDef => LoadedProjectileDef;

        public int BaseReloadTicks => 100;

        public int MaxCharges => 1;

        public string LabelRemaining => $"Charges {RemainingCharges}";

        private int charges = 1;
        public int RemainingCharges => charges;


        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (charges > 0)
            {
                if (this.LoadedProjectileDef != null)
                {
                    Pawn pawn = this.parent.pawn;
                    ((Projectile)GenSpawn.Spawn(this.LoadedProjectileDef, pawn.Position, pawn.Map, WipeMode.Vanish)).Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget, false, null, null);

                    UseCharges(1);
                }
            }
            else
            {
       
                List<FloatMenuGridOption> options = new List<FloatMenuGridOption>();

                foreach (var item in this.parent.pawn.Map.listerThings.AllThings)
                {

                    if (item.def.projectileWhenLoaded != null)
                    {
                        Log.Message($"{item.def.LabelCap}");
                        options.Add(new FloatMenuGridOption(item.def.uiIcon, () =>
                        {
                            Job job = JobGiver_Reload.MakeReloadJob(this, new List<Thing>() { item });
                            this.parent.pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                            //ReloadFrom(item);
                        }, null, new TipSignal($"Load {item.def.LabelCap}")));
                    }


                }

                Log.Message($"OPTIONS{options.Count}");
                Find.WindowStack.Add(new FloatMenuGrid(options));
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (this.parent.OnCooldown && this.charges <= 0)
            {
                reason = "";
                return false;
            }
            else
            {
                return base.GizmoDisabled(out reason);
            }

        }

        public virtual void AddCharge(int amount)
        {
            charges += amount;

            if (charges > 0)
            {
                OnChargesGained();
            }
        }

        public virtual void UseCharges(int amount)
        {
            charges -= amount;

            if (charges <= 0)
            {
                charges = 0;
                OnChargesDepleted();
            }
        }

        public virtual void OnChargesGained()
        {
           // this.parent.verb.targetParams.canTargetSelf = false;
           // this.parent.verb.verbProps.targetable = true;
        }

        public virtual void OnChargesDepleted()
        {
           // this.parent.verb.targetParams.canTargetSelf = true;
           // this.parent.verb.verbProps.targetable = false;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }

        public bool NeedsReload(bool allowForceReload)
        {
            return charges < MaxCharges || charges <= 0;
        }

        public int MinAmmoNeeded(bool allowForcedReload)
        {
            return 1;
        }

        public int MaxAmmoNeeded(bool allowForcedReload)
        {
            return MaxCharges;
        }

        public int MaxAmmoAmount()
        {
            return MaxCharges;
        }

        public void ReloadFrom(Thing ammo)
        {
            if (charges > MaxCharges)
            {
                return;
            }


            if (ammo.stackCount > 0)
            {
                Thing chosen = ammo.SplitOff(1);
                LoadedProjectileDef = chosen.def;
                AddCharge(1);
                if (!chosen.Destroyed)
                {
                    chosen.Destroy();
                }


            }
        }

        public string DisabledReason(int minNeeded, int maxNeeded)
        {
            return $"Needs Reloading";
        }

        public bool CanBeUsed(out string reason)
        {
            reason = "";

            if (charges <= 0)
            {
                reason = "No Charges";
                return false;
            }

            if (LoadedProjectileDef == null)
            {
                reason = "No Projectile Loaded";
                return false;
            }

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref charges, "charges");
            Scribe_Defs.Look(ref LoadedProjectileDef, "loadedProjectile");
        }
    }
}
