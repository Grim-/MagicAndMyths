using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ChainLightning : CompProperties_AbilityEffect
    {
        public bool lightning = true;
        public int lifetimeTicks = 500;
        public int maxJumpTargets = 5;
        public float targetJumpRadius = 12f;
        public int ticksBetweenJumps = 10;
        public int lingerTicks = 30;
        public int damageAmount;
        public DamageDef damageDef;
        public SoundDef soundOnImpact;

        public CompProperties_ChainLightning()
        {
            compClass = typeof(CompAbilityEffect_ChainLightning);
        }
    }

    public class CompAbilityEffect_ChainLightning : CompAbilityEffect
    {
        new CompProperties_ChainLightning Props => (CompProperties_ChainLightning)props;
        private StaggeredChainLightning chainLightning;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);


            if (parent.pawn?.Map == null)
                return;

            if (chainLightning != null)
            {
                chainLightning.Stop();
                chainLightning = null;
            }

            DamageDef damageDef = Props.damageDef != null ? Props.damageDef : DamageDefOf.ElectricalBurn;


           chainLightning = new StaggeredChainLightning(parent.pawn?.Map, parent.pawn, Props.lifetimeTicks, Props.maxJumpTargets, Props.targetJumpRadius, Props.damageAmount, damageDef, (Thing) =>
            {
                return Thing != this.parent.pawn;
            }, Props.ticksBetweenJumps, Props.lingerTicks);


            chainLightning.StartChain(target.Pawn);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (chainLightning != null)
            {
                chainLightning.Tick();

                if (chainLightning.IsFinished)
                {
                    chainLightning = null;
                }
            }

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref chainLightning, "chainLightning");
        }
    }
}
