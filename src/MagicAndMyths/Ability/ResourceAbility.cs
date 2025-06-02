using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class ResourceAbilityDef : AbilityDef
    {
        public PawnResourceDef resourceDef;
        public float resourceCost = 10f;

        public ResourceAbilityDef()
        {
            abilityClass = typeof(ResourceAbility);
        }
    }

    public class ResourceAbility : Ability
    {
        public ResourceAbility()
        {

        }

        public ResourceAbility(Pawn pawn) : base(pawn)
        {

        }

        public ResourceAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept)
        {

        }

        public ResourceAbility(Pawn pawn, AbilityDef def) : base(pawn, def)
        {

        }

        public ResourceAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def)
        {

        }

        public ResourceAbilityDef ResourceDef => (ResourceAbilityDef)def;
        protected Gene_BasicResource resourceGene => this.pawn.GetGeneForResourceDef(ResourceDef.resourceDef);
        public override bool CanCast
        {
            get
            {
                if (!base.CanCast)
                    return false;

                if (ResourceDef.resourceDef == null)
                    return true;
                if (resourceGene == null)
                    return false;
                if (resourceGene.ResourceIsUnavailable(out string reason))
                    return false;

                return resourceGene.Has(ResourceDef.resourceCost);
            }
        }
        public override bool CanQueueCast
        {
            get
            {
                if (!base.CanQueueCast)
                    return false;

                if (ResourceDef.resourceDef == null)
                    return true;
                if (resourceGene == null)
                    return false;
                if (resourceGene.ResourceIsUnavailable(out string reason))
                    return false;

                return resourceGene.Has(ResourceDef.resourceCost);
            }
        }

        protected override void PreActivate(LocalTargetInfo? target)
        {
            base.PreActivate(target);
            ConsumeResource();
        }

        protected virtual void ConsumeResource()
        {
            if (ResourceDef.resourceDef == null)
                return;

            if (resourceGene != null && resourceGene.Has(ResourceDef.resourceCost))
            {
                resourceGene.Consume(ResourceDef.resourceCost);
            }
        }
    }
}
