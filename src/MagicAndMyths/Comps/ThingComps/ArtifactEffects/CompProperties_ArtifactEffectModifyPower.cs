using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectModifyPower : CompProperties
    {
        public bool addsPower = true;
        public FloatRange amount = new FloatRange(1f, 1f);

        public CompProperties_ArtifactEffectModifyPower()
        {
            compClass = typeof(Comp_ArtifactEffectModifyPower);
        }
    }

    public class Comp_ArtifactEffectModifyPower : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectModifyPower Props => (CompProperties_ArtifactEffectModifyPower)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (target.Thing != null)
            {
                if (target.Thing.TryGetComp(out CompPowerBattery compPowerBattery))
                {
                    float chosenValue = Props.amount.RandomInRange;
                    if (Props.addsPower)
                    {
                        compPowerBattery.AddEnergy(chosenValue);
                    }
                    else
                    {
                        compPowerBattery.DrawPower(chosenValue);
                    }
                }
            }
        }
    }
}