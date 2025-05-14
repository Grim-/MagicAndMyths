using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_StarPull : CompProperties_AbilityEffect
    {
        public int radius = 15;
        public EffecterDef effecterDef;

        public HediffDef hediffDef = null;

        public CompProperties_StarPull()
        {
            compClass = typeof(CompAbilityEffect_StarPull);
        }
    }


    public class CompAbilityEffect_StarPull : CompAbilityEffect
    {
        CompProperties_StarPull Props => (CompProperties_StarPull)props;

        WeaponAbility WeaponAbility => this.parent as WeaponAbility;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            if (Props.effecterDef != null)
            {
                Props.effecterDef.Spawn(this.parent.pawn.Position, this.parent.pawn.Map);
            }

            List<Pawn> pawnsInRange = TargetUtil.GetPawnsInRadius(this.parent.pawn.Position, map, Props.radius, this.parent.pawn.Faction, true, this.parent.pawn, true, false, false);

            foreach (var pawn in pawnsInRange)
            {
                if (pawn == this.parent.pawn)
                {
                    continue;
                }

                if (Props.hediffDef != null)
                {
                    pawn.health.GetOrAddHediff(Props.hediffDef);
                }

                ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, pawn, this.parent.pawn.Position, map, null, null, this.parent.pawn, pawn.DrawPos, false);
                ThingFlyer.LaunchFlyer(thingFlyer, pawn, pawn.Position, map);
            }
        }
    }


}
