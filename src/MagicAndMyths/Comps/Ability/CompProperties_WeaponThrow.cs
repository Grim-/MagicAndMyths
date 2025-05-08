using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace MagicAndMyths
{
    public class CompProperties_WeaponThrow : CompProperties_AbilityEffect
    {
        public CompProperties_WeaponThrow()
        {
            compClass = typeof(Comp_WeaponThrow);
        }
    }
    public class Comp_WeaponThrow : CompAbilityEffect
    {
        new CompProperties_WeaponThrow Props => (CompProperties_WeaponThrow)props;
        private ThingWithComps thrownWeapon;
        private ThingFlyer currentThingFlyer;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return thrownWeapon != null || parent.pawn.equipment?.Primary != null && base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            if (pawn == null || !pawn.Spawned)
                return;

            if (thrownWeapon != null && thrownWeapon.Spawned)
            {
                RecallWeapon(pawn);
                this.parent.verb.verbProps.targetable = true;
            }
            else if (thrownWeapon != null && pawn.equipment?.Primary == thrownWeapon)
            {
                ThrowWeapon(pawn, target);
                this.parent.verb.verbProps.targetable = false;
            }
            else if (thrownWeapon == null && pawn.equipment?.Primary != null)
            {
                ThrowWeapon(pawn, target);
                this.parent.verb.verbProps.targetable = false;
            }
        }

        private void ThrowWeapon(Pawn pawn, LocalTargetInfo target)
        {
            ThingWithComps weapon = pawn.equipment?.Primary;
            if (weapon == null)
                return;

            CleanupFlyer();

            thrownWeapon = weapon;
            pawn.equipment.Remove(weapon);

            currentThingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, thrownWeapon, target.Cell, pawn.Map, null, null, pawn, null, true);
            currentThingFlyer = ThingFlyer.LaunchFlyer(currentThingFlyer, thrownWeapon, pawn.Position, pawn.Map);
            currentThingFlyer.OnRespawn += ThrowWeapon_OnRespawn;
        }

        private void ThrowWeapon_OnRespawn(IntVec3 position, Thing thing, Pawn pawn)
        {
            CleanupFlyer();
        }

        private void RecallWeapon(Pawn pawn)
        {
            if (thrownWeapon == null || !thrownWeapon.Spawned)
                return;

            CleanupFlyer();

            currentThingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, thrownWeapon, this.parent.pawn.Position, this.parent.pawn.Map, null, null, this.parent.pawn, null,  false);
            currentThingFlyer = ThingFlyer.LaunchFlyer(currentThingFlyer, thrownWeapon, thrownWeapon.Position, this.parent.pawn.Map);
            currentThingFlyer.OnRespawn += OnRecallWeaponLanded;
        }

        private void OnRecallWeaponLanded(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            if (thrownWeapon != null)
            {
                if (thrownWeapon.Spawned)
                {
                    thrownWeapon.DeSpawn();
                }

                this.parent.pawn.equipment.AddEquipment(thrownWeapon);
                thrownWeapon = null;
            }

            CleanupFlyer();
        }

        private void CleanupFlyer()
        {
            if (currentThingFlyer != null)
            {
                currentThingFlyer.OnRespawn -= ThrowWeapon_OnRespawn;
                currentThingFlyer.OnRespawn -= OnRecallWeaponLanded;
                currentThingFlyer = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref thrownWeapon, "thrownWeapon");
        }
    }
}