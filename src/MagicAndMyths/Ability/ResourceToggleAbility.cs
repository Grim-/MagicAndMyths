using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class ResourceToggleAbilityDef : ResourceAbilityDef
    {
        public float resourceMaintainCost = 0;
        public int resourceMaintainInterval = 300;

        public ResourceToggleAbilityDef()
        {
            abilityClass = typeof(ResourceToggleAbility);
        }
    }

    public class ResourceToggleAbility : ResourceAbility, IToggleableAbility
    {

        public ResourceToggleAbility()
        {

        }

        public ResourceToggleAbility(Pawn pawn) : base(pawn)
        {

        }

        public ResourceToggleAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept)
        {

        }

        public ResourceToggleAbility(Pawn pawn, AbilityDef def) : base(pawn, def)
        {

        }

        public ResourceToggleAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def)
        {

        }
        new public ResourceToggleAbilityDef ResourceDef => (ResourceToggleAbilityDef)def;

        protected bool IsActive = false;

        public override bool CanCast
        {
            get
            {
                //if it has a cooldown and the toggle is active, allow deactivating regardless of cooldown
                if (this.OnCooldown && IsActive)
                {
                    return true;
                }
                else return base.CanCast;
            }
        }

        public override bool CanQueueCast
        {
            get
            {
                //if it has a cooldown and the toggle is active, allow deactivating regardless of cooldown
                if (this.OnCooldown && IsActive)
                {
                    return true;
                }
                else return base.CanQueueCast;
            }
        }
        public override string Tooltip
        {
            get
            {

                if (ResourceDef != null && ResourceDef.resourceDef != null)
                {
                    return base.Tooltip + $"\r\nMaintain Cost : {ResourceDef.resourceMaintainCost} ({ResourceDef.resourceDef.LabelCap}) every {ResourceDef.resourceMaintainInterval.ToStringTicksToPeriod()}.";
                }

                return base.Tooltip;
            }
        }

        public ResourceToggleAbilityDef ToggleDef => ResourceDef;

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (IsActive)
            {
                DeActivate();
            }
            else
            {
                Activate();
            }
            return base.Activate(target, dest);
        }

        public override void AbilityTick()
        {
            base.AbilityTick();

            if (IsActive)
            {
                if (this.pawn.IsHashIntervalTick(ToggleDef.resourceMaintainInterval))
                {
                    if (!resourceGene.Has(ResourceDef.resourceMaintainCost))
                    {
                        DeActivate();
                    }
                    else
                    {
                        resourceGene.Consume(ResourceDef.resourceDef, ResourceDef.resourceMaintainCost);
                    }
                }
            }
        }

        protected override void ConsumeResource()
        {
            //no resource cost to deactivate, by default
            if (!IsActive)
            {
                base.ConsumeResource();
            }  
        }


        public void Activate(bool force = false)
        {
            if (IsActive && !force)
            {
                return;
            }

            IsActive = true;
            OnActivated();
        }

        public void DeActivate(bool force = false)
        {
            if (!IsActive && !force)
            {
                return;
            }

            IsActive = false;
            OnDeactivated();
        }


        protected virtual void OnActivated()
        {
            foreach (var item in this.CompsOfType<BaseToggleAbilityComp>())
            {
                item.OnParentActivated();
            }
        }

        protected virtual void OnDeactivated()
        {
            foreach (var item in this.CompsOfType<BaseToggleAbilityComp>())
            {
                item.OnParentDeactivated();
            }
        }
    }
}
