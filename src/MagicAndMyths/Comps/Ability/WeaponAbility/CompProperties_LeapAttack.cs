using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_LeapAttack : CompProperties_BaseWeaponAbility
    {
        public float radius = 3f;
        public EffecterDef slamEffecterDef = null;
        public CompProperties_LeapAttack()
        {
            compClass = typeof(CompAbilityEffect_LeapAttack);
        }
    }

    public class CompAbilityEffect_LeapAttack : CompAbilityEffect_BaseWeaponAbility
    {
        IntVec3 startcell;
        IntVec3 targetCell;
        ThingFlyer thingFlyer;
        bool pawnWasDrafted = false;
        bool pawnWasSelected = false;
        CompProperties_LeapAttack Props => (CompProperties_LeapAttack)props;

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
            pawnWasDrafted = this.parent.pawn.Drafted;
            pawnWasSelected = Find.Selector.AnyPawnSelected && Find.Selector.SingleSelectedThing is Pawn selectedPawn && selectedPawn == this.parent.pawn;
            thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, this.parent.pawn, targetCell, map, null, null, this.parent.pawn, this.parent.pawn.DrawPos, false);
            thingFlyer = ThingFlyer.LaunchFlyer(thingFlyer, this.parent.pawn, startcell, map);

            thingFlyer.OnRespawn += ThingFlyer_OnRespawn;
        }

        private void ThingFlyer_OnRespawn(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            if (Props.slamEffecterDef != null)
            {
                Props.slamEffecterDef.Spawn(arg1, this.parent.pawn.Map);
            }

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.radius, true).ToList();

            cells = cells.OrderBy(x => x.DistanceTo(this.parent.pawn.Position)).ToList();

            cells.ForEach(x => EffecterDefOf.ImpactSmallDustCloud.Spawn(x, arg3.Map));

            foreach (var pawn in TargetUtil.GetPawnsInRadius(arg1, this.parent.pawn.Map, Props.radius, this.parent.pawn.Faction, true, this.parent.pawn, true, true, true))
            {
                if (pawn != null && pawn != this.parent.pawn)
                {
                    pawn.TakeDamage(this.parent.pawn.equipment.PrimaryEq.GetWeaponDamage(this.parent.pawn));
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
            GenDraw.DrawCircleOutline(target.Cell.ToVector3Shifted(), Props.radius);
        }
    }
}
