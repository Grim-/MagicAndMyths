using RimWorld;
using System.Collections.Generic;
using System;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ResourceAbilityDef : AbilityDef
    {
        public AbilityResourceDef resourceDef;
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

        public override string Tooltip
        {
            get
            {
                if (ResourceDef != null && ResourceDef.resourceDef != null)
                {
                    return base.Tooltip + $"\r\nCost : {ResourceDef.resourceCost} ({ResourceDef.resourceDef.LabelCap})";
                }
                return base.Tooltip;
            }
        }

        public override bool CanCast
        {
            get
            {
                if (!base.CanCast)
                    return false;
                if (ResourceDef == null || ResourceDef.resourceDef == null)
                    return true;
                if (resourceGene == null)
                    return false;
                if (resourceGene.ResourceIsUnavailable(out string reason))
                    return false;
                return resourceGene.Has(ResourceDef.resourceDef, ResourceDef.resourceCost);
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
                return resourceGene.Has(ResourceDef.resourceDef, ResourceDef.resourceCost);
            }
        }

        new public virtual void Initialize()
        {
            if (this.def.comps.Any<AbilityCompProperties>())
            {
                this.comps = new List<AbilityComp>();
                for (int i = 0; i < this.def.comps.Count; i++)
                {
                    AbilityComp abilityComp = null;
                    try
                    {
                        abilityComp = (AbilityComp)Activator.CreateInstance(this.def.comps[i].compClass);
                        abilityComp.parent = this;
                        this.comps.Add(abilityComp);
                        abilityComp.Initialize(this.def.comps[i]);
                    }
                    catch (Exception arg)
                    {
                        Log.Error("Could not instantiate or initialize an AbilityComp: " + arg);
                        this.comps.Remove(abilityComp);
                    }
                }
            }
            if (this.Id == -1)
            {
                this.Id = Find.UniqueIDsManager.GetNextAbilityID();
            }
            IAbilityVerb abilityVerb;
            if ((abilityVerb = (this.VerbTracker.PrimaryVerb as IAbilityVerb)) != null)
            {
                abilityVerb.Ability = this;
            }
            if (this.def.charges > 0)
            {
                this.maxCharges = this.def.charges;
                this.RemainingCharges = this.maxCharges;
            }
        }

        protected override void PreActivate(LocalTargetInfo? target)
        {
            base.PreActivate(target);
            ConsumeResource();
        }

        public virtual float GetDamageScalingMultiplier()
        {
            if (ResourceDef == null || ResourceDef.resourceDef == null)
            {
                return 1;
            }

            return Mathf.Max(1, ResourceDef.resourceDef.damageScalingStat != null ? this.pawn.GetStatValue(ResourceDef.resourceDef.damageScalingStat) : 1f);
        }

        public virtual DamageInfo GetModifiedDamage(DamageInfo damageInfo)
        {
            float scalingMultiplier = GetDamageScalingMultiplier();

            DamageInfo modifiedDamage = damageInfo;
            modifiedDamage.SetAmount(damageInfo.Amount * scalingMultiplier);

            Log.Message($"Scaled Damage to {modifiedDamage.Amount} from {damageInfo.Amount}");
            return modifiedDamage;
        }

        public virtual float GetModifiedDamageAmount(float baseDamage)
        {
            return baseDamage * GetDamageScalingMultiplier();
        }

        public virtual float GetModifiedHealAmount(float baseHealAmount)
        {
            return baseHealAmount * GetDamageScalingMultiplier();
        }

        protected virtual void ConsumeResource()
        {
            if (ResourceDef.resourceDef != null && resourceGene != null && resourceGene.Has(ResourceDef.resourceDef, ResourceDef.resourceCost))
            {
                resourceGene.Consume(ResourceDef.resourceCost);
            }
        }
    }
}