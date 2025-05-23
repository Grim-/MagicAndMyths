﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{


    //public class Trail : Thing
    //{
    //    MoteDualAttached mote = null;
    //    private TargetInfo Target = null;
    //    private float Width = 2f;
    //    private Map Map = null;
    //    private Vector3 originOffset;
    //    private Vector3 targetOffset;

    //    private int LifetimeTicks = 0;
    //    private int TicksToLive = 500;

    //    public void Init(ThingDef effecterDef, TargetInfo target, Map map, float width = 2f, int ticksToLive = 500, Vector3 originOffset = default(Vector3), Vector3 targetOffset = default(Vector3))
    //    {
    //        Target = target;
    //        Map = map;
    //        Width = width;
    //        TicksToLive = ticksToLive;
    //        mote = (MoteDualAttached)ThingMaker.MakeThing(effecterDef, null);
    //        GenSpawn.Spawn(mote, target.Cell, map, WipeMode.Vanish);
    //        mote.Attach(new TargetInfo(this.Position, map, false), target);
    //        mote.linearScale = new Vector3(width, 1f, (this.DrawPos - target.Cell.ToVector3Shifted()).MagnitudeHorizontal());
    //    }

    //    public override void Tick()
    //    {
    //        if (!Spawned)
    //        {
    //            return;
    //        }

    //        LifetimeTicks++;
    //        if (LifetimeTicks >= TicksToLive)
    //        {
    //            this.Destroy();
    //            return;
    //        }

    //        if (mote != null)
    //        {
    //            mote.Maintain();
    //            mote.UpdateTargets(new TargetInfo(this.Position, Map, false), Target, originOffset != default(Vector3) ? originOffset : Vector3.zero, targetOffset != default(Vector3) ? targetOffset : Vector3.zero);
    //            mote.linearScale = new Vector3(Width, 1f, (this.DrawPos - Target.Cell.ToVector3Shifted()).MagnitudeHorizontal());
    //        }
    //    }


    //    public static Trail PlaceTrail(ThingDef effecterDef, IntVec3 spawnPosition, TargetInfo target, Map map, float width = 2f, int ticksToLive = 500, Vector3 originOffset = default(Vector3), Vector3 targetOffset = default(Vector3))
    //    {

    //    }
    //}

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
                GenSpawn.Spawn(mote, this.parent.pawn.Position, map, WipeMode.Vanish);
                mote.Attach(new TargetInfo(this.parent.pawn.Position, map, false), new TargetInfo(targetCell, map, false));
                mote.linearScale = new Vector3(4f, 1f, (this.parent.pawn.DrawPos - targetCell.ToVector3Shifted()).MagnitudeHorizontal());
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
