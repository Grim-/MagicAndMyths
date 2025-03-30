using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_EnergyComp : HediffCompProperties
    {
        public float aoeRadius = 4f;
        public float teleportCost = 50f;
        public float maxEnergy = 100f;
        public StatDef maxEnergyStat;


        public StatDef regenStat;
        public float chargePerHour = 8;

        public bool hasNaturalRegeneration = true;


        public HediffCompProperties_EnergyComp()
        {
            compClass = typeof(HediffComp_EnergyComp);
        }
    }

    public class HediffComp_EnergyComp : HediffComp, IEnergyProvider
    {
        private float energy;
        public virtual float Energy => energy;
        public virtual float MaxEnergy => this.Props.maxEnergyStat != null ? this.Pawn.GetStatValue(this.Props.maxEnergyStat) : Props.maxEnergy;

        public virtual float RegenPerHour => this.Props.regenStat != null ? this.Pawn.GetStatValue(this.Props.regenStat) : Props.chargePerHour;

        public HediffCompProperties_EnergyComp Props => (HediffCompProperties_EnergyComp)props;
        public override void CompPostMake()
        {
            base.CompPostMake();

            energy = MaxEnergy;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Props.hasNaturalRegeneration)
            {
                float chargePerTick = RegenPerHour / GenDate.TicksPerHour;
                AddEnergy(chargePerTick);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            yield return new Gizmo_EnergyStatus
            {
                thing = parent.pawn,
                EnergyComp = this,
                customLabel = "BioEnergy",
                barColor = new Color(0.2f, 0.6f, 0.9f),
            };
        }

        public virtual bool TryUseEnergy(float amount)
        {
            if (energy >= amount)
            {
                energy -= amount;
                return true;
            }
            return false;
        }
        public virtual void AddEnergy(float amount)
        {
            energy = Mathf.Min(MaxEnergy, energy + amount);
        }

        public virtual bool HasEnough(float amount)
        {
            return energy >= amount;
        }

        public virtual float GetEnergyPercent()
        {
            return energy / MaxEnergy;
        }
    }
}