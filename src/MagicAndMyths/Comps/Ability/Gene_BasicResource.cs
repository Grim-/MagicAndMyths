﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class BasicResourceGeneDef : GeneDef
    {
        public string resourceName = "unnamed resource";
        public StatDef maxStat;
        public StatDef regenTicks;
        public StatDef regenStat;
        public StatDef regenSpeedStat;
        public StatDef costMult;
        public Color barColor = Color.cyan;

        public BasicResourceGeneDef()
        {
            geneClass = typeof(Gene_BasicResource);
            this.resourceGizmoType = typeof(GeneGizmo_BasicResource);
        }
    }
    [StaticConstructorOnStartup]
    public class GeneGizmo_BasicResource : GeneGizmo_Resource
    {
        private const float TotalPulsateTime = 0.85f;
        private List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();

        protected override string Title
        {
            get
            {
                if (gene is Gene_BasicResource resourceGene && resourceGene.Def != null)
                {
                    return resourceGene.Def.resourceLabel;
                }
                return base.Title;
            }
        }

        public GeneGizmo_BasicResource(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor) : base(gene, drainGenes, barColor, barHighlightColor)
        {
            if (gene == null)
            {
                Log.Error("GeneGizmo_BasicResource created with null gene");
                return;
            }
        }

        protected override string GetTooltip()
        {
            if (!(gene is Gene_BasicResource resourceGene)) return "";

            string text = $"{resourceGene.Def.resourceLabel.CapitalizeFirst()}: {resourceGene.ValueForDisplay} / {resourceGene.MaxForDisplay}\n";

            string regen = $"\nRegenerates {resourceGene.RegenAmount} every {GenDate.ToStringTicksToPeriod(resourceGene.RegenTicks)}";

            if (!resourceGene.def.resourceDescription.NullOrEmpty())
            {
                text += $"\n\n{resourceGene.def.resourceDescription.Formatted(resourceGene.pawn.Named("PAWN")).Resolve()}";
            }

            return text + regen;
        }
    }
    public class Gene_BasicResource : Gene_Resource, IGeneResourceDrain
    {
        public BasicResourceGeneDef Def => def != null ? (BasicResourceGeneDef)def : null;

        public bool EnableResource = true;
        public Gene_Resource Resource => this;
        public Pawn Pawn => pawn;

        public bool CanOffset
        {
            get
            {
                if (Active)
                {
                    return !pawn.Deathresting;
                }
                return false;
            }
        }

        private int CurrentTick = 0;

        public override float Value
        {
            get => base.Value;
            set => base.Value = Mathf.Clamp(value, 0f, Max);
        }

        public float ValueCostMultiplied => Value * CostMult;
        public string DisplayLabel => Def.resourceName + " (" + "Gene".Translate() + ")";
        public float ResourceLossPerDay => def?.resourceLossPerDay ?? 0f;
        public override float InitialResourceMax => Def?.maxStat != null ? Pawn.GetStatValue(Def.maxStat, true, 1250) : 100f;
        public override float MinLevelForAlert => 0.15f;
        public override float MaxLevelOffset => 0.1f;

        private float lastMax;
        public override float Max
        {
            get
            {
                if (Def?.maxStat == null) return 10f;
                float currentMax = Pawn.GetStatValue(Def.maxStat, true, 1250);
                if (currentMax != lastMax)
                {
                    lastMax = currentMax;
                    ForceBaseMaxUpdate(currentMax);
                }
                return currentMax;
            }
        }

        protected override Color BarColor => Def?.barColor ?? new ColorInt(3, 3, 138).ToColor;
        protected override Color BarHighlightColor => new ColorInt(42, 42, 145).ToColor;

        public override int ValueForDisplay => Mathf.RoundToInt(Value);
        public override int MaxForDisplay => Mathf.RoundToInt(Max);

        public float RegenAmount => Def?.regenStat != null ? Pawn.GetStatValue(Def.regenStat, true, 100) : 1f;
        public float RegenSpeed => Def?.regenSpeedStat != null ? Pawn.GetStatValue(Def.regenSpeedStat, true, 100) : 1f;
        public int RegenTicks => Def?.regenTicks != null ? Mathf.RoundToInt(Pawn.GetStatValue(Def.regenTicks, true, 100)) : 1250;
        public float CostMult => Def?.costMult != null ? Pawn.GetStatValue(Def.costMult, true, 100) : 1f;

        public float TotalResourceUsed = 0;

        public override void PostAdd()
        {
            if (ModLister.CheckBiotech("Hemogen"))
            {
                base.PostAdd();
                Reset();
            }
        }

        private void ForceBaseMaxUpdate(float newMax)
        {
            this.SetMax(newMax);
        }


        public void Consume(float Amount)
        {
            if (!ModsConfig.BiotechActive) return;
            TotalResourceUsed += Amount;
            Value -= Amount * CostMult;
        }

        public void Restore(float Amount)
        {
            if (!ModsConfig.BiotechActive) return;
            Value += Amount;
        }

        public bool Has(float Amount)
        {
            if (!ModsConfig.BiotechActive) return false;
            return Value >= Amount * CostMult;
        }

        public override void Tick()
        {
            base.Tick();
            CurrentTick++;
            if (CurrentTick >= RegenTicks)
            {
                Restore(RegenAmount);
                ResetRegenTicks();
            }
        }

        public void ResetRegenTicks()
        {
            CurrentTick = 0;
        }

        public override void SetTargetValuePct(float val)
        {
            targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
        }

        public bool ShouldConsumeHemogenNow()
        {
            return Value < targetValue;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableResource, "resourceEnabled", defaultValue: true);
            Scribe_Values.Look(ref CurrentTick, "currentRegenTick", defaultValue: 0);
            Scribe_Values.Look(ref TotalResourceUsed, "TotalResourceUsed", defaultValue: 0);
        }
    }
}
