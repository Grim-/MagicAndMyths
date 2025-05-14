using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ChargeAttack : CompProperties_AbilityEffect
    {
        public int width = 3;
        public int length = 12;
        public ThingDef attackEffectDef;

        public HediffDef hediffToApply = null;
        public FloatRange chanceToApply = new FloatRange(1f, 1f);

        public CompProperties_ChargeAttack()
        {
            compClass = typeof(CompAbilityEffect_ChargeAttack);
        }
    }

    public class CompAbilityEffect_ChargeAttack : CompAbilityEffect
    {
        IntVec3 startcell;
        IntVec3 targetCell;
        ThingFlyer thingFlyer;

        bool pawnWasDrafted = false;
        bool pawnWasSelected = false;

        CompProperties_ChargeAttack Props => (CompProperties_ChargeAttack)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (thingFlyer != null)
            {
                thingFlyer.OnRespawn -= ThingFlyer_OnRespawn;
                thingFlyer = null;
            }

            Map map = this.parent.pawn.Map;
            startcell = this.parent.pawn.Position;
            targetCell = target.Cell;

            if (Props.attackEffectDef != null)
            {
                MoteDualAttached mote = (MoteDualAttached)ThingMaker.MakeThing(Props.attackEffectDef, null);
                mote.Scale = 2f;
                mote.Attach(new TargetInfo(this.parent.pawn.Position, map, false), new TargetInfo(targetCell, map, false));
                GenSpawn.Spawn(mote, this.parent.pawn.Position, map, WipeMode.Vanish);
            }

            pawnWasDrafted = this.parent.pawn.Drafted;
            pawnWasSelected = Find.Selector.AnyPawnSelected && Find.Selector.SingleSelectedThing is Pawn selectedPawn && selectedPawn == this.parent.pawn;
      
            thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, this.parent.pawn, targetCell, map, null, null, this.parent.pawn, this.parent.pawn.DrawPos, false);
            thingFlyer = ThingFlyer.LaunchFlyer(thingFlyer, this.parent.pawn, startcell, map);

            thingFlyer.OnRespawn += ThingFlyer_OnRespawn;
        }

        private void ThingFlyer_OnRespawn(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            foreach (var item in TargetUtil.GetAllCellsInRect(startcell, targetCell, Props.width, Props.length))
            {
                Pawn pawn = item.GetFirstPawn(this.parent.pawn.Map);
                if (pawn != null && pawn != this.parent.pawn)
                {
                    DamageInfo damage = this.parent.pawn.equipment.PrimaryEq.GetWeaponDamage(this.parent.pawn);

                    pawn.TakeDamage(damage);

                    if (Rand.Value >= Props.chanceToApply.RandomInRange)
                    {
                        if (Props.hediffToApply != null)
                        {
                            pawn.health.GetOrAddHediff(Props.hediffToApply, null, damage);
                        }

                    }
                }
            }

            if (pawnWasDrafted)
            {
                this.parent.pawn.drafter.Drafted = true;
            }

            if (pawnWasSelected)
            {
                Find.Selector.Select(this.parent.pawn, false);
            }
          
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);


            GenDraw.DrawFieldEdges(TargetUtil.GetAllCellsInRect(this.parent.pawn.Position, target.Cell, Props.width, Props.length));
        }


    }


}
