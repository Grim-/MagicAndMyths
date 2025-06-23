using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PowerManaCore : CompProperties_Battery
    {
        public float passiveEnergyGainPerTick = 0.1f;
        public bool selfDischarge = false;

        public CompProperties_PowerManaCore()
        {
            this.compClass = typeof(CompPowerManaCore);
        }
    }

    public class CompPowerManaCore : CompPowerBattery
    {
        private int lastPassiveRegenTick = -1;

        public new CompProperties_PowerManaCore Props
        {
            get
            {
                return (CompProperties_PowerManaCore)this.props;
            }
        }



        protected MapComp_ManaManager _ManaManager;


        protected MapComp_ManaManager ManaManager
        {
            get
            {
                if (_ManaManager == null)
                {
                    _ManaManager = this.parent.Map.GetComponent<MapComp_ManaManager>();
                }

                return _ManaManager;
            }
        }

        public bool SuppressedByAntiMagic
        {
            get
            {
                return !ManaManager.IsManaEnabled;
            }
        }

        public override void CompTick()
        {
            if (!this.SuppressedByAntiMagic && !this.parent.IsBrokenDown())
            {
                this.PassiveEnergyGeneration();
            }

            if (!this.Props.selfDischarge)
            {
                return;
            }

            base.CompTick();
        }

        private void PassiveEnergyGeneration()
        {
            if (Find.TickManager.TicksGame != this.lastPassiveRegenTick)
            {
                this.lastPassiveRegenTick = Find.TickManager.TicksGame;

                if (this.StoredEnergy < this.Props.storedEnergyMax)
                {
                    float passiveGain = this.Props.passiveEnergyGainPerTick * (ManaManager != null ? ManaManager.ManaFlowMultiplier : 1);
                    this.AddEnergy(passiveGain);
                }
            }
        }

        public void AddEnergyFromFueling(float amount)
        {
            this.AddEnergy(amount);
        }

        new public void AddEnergy(float amount)
        {
            if (this.SuppressedByAntiMagic)
            {
                return;
            }
            base.AddEnergy(amount);
        }

        public override string CompInspectStringExtra()
        {
            CompProperties_PowerManaCore props = this.Props;
            string text = "ManaStored".Translate() + ": " + this.StoredEnergy.ToString("F0") + " / " + props.storedEnergyMax.ToString("F0") + " " + "ManaUnits".Translate();

            if (!this.SuppressedByAntiMagic && !this.parent.IsBrokenDown())
            {
                text += "\n" + "PassiveGeneration".Translate() + ": " + (props.passiveEnergyGainPerTick * 60000f).ToString("F1") + " " + "ManaUnitsPerDay".Translate();
            }

            if (this.SuppressedByAntiMagic)
            {
                text += "\n" + "SuppressedByAntiMagic".Translate();
            }

            return text + "\n" + base.CompInspectStringExtra();
        }
    }
}