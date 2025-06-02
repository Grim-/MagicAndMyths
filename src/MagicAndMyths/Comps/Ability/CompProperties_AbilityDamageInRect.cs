using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityDamageInRect : CompProperties_AbilityEffect
    {

        public int length = 10;
        public int width = 3;

        public FloatRange damage;
        public DamageDef damageDef;

        public ThingDef effectMote = null;

        public CompProperties_AbilityDamageInRect()
        {
            compClass = typeof(CompAbilityEffect_AbilityDamageInRect);
        }
    }
    public class CompAbilityEffect_AbilityDamageInRect : CompAbilityEffect
    {
        public new CompProperties_AbilityDamageInRect Props => (CompProperties_AbilityDamageInRect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            IntVec3 startcell = this.parent.pawn.Position;
            IntVec3 targetCell = target.Cell;

            if (Props.effectMote != null)
            {
                MoteDualAttached mote = (MoteDualAttached)ThingMaker.MakeThing(Props.effectMote, null);
                GenSpawn.Spawn(mote, this.parent.pawn.Position, map, WipeMode.Vanish);
                mote.Attach(new TargetInfo(this.parent.pawn.Position, map, false), new TargetInfo(target.Cell, map, false));
                mote.linearScale = new Vector3(4f, 1f, (this.parent.pawn.DrawPos - targetCell.ToVector3Shifted()).MagnitudeHorizontal());
            }

            List<IntVec3> cells = TargetUtil.GetAllCellsInRect(this.parent.pawn.Position, target.Cell, Props.width, Props.length);

            foreach (var item in TargetUtil.GetDamageableThingsInCells(cells, this.parent.pawn.Map))
            {
                if (item != this.parent.pawn)
                {
                    item.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
                }         
            }
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            GenDraw.DrawFieldEdges(TargetUtil.GetAllCellsInRect(this.parent.pawn.Position, target.Cell, Props.width, Props.length));
        }
    }

}
