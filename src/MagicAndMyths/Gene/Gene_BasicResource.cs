using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Gene_BasicResource : Gene_Resource, IGeneResourceDrain
    {
        public BasicResourceGeneDef Def => def != null ? (BasicResourceGeneDef)def : null;
        public PawnResourceDef ResourceDef => Def?.resourceDef;

        public bool EnableResource = true;
        public Gene_Resource Resource => this;
        public Pawn Pawn => pawn;

        public bool CanOffset
        {
            get
            {
                if (IsLocked)
                {
                    return false;
                }

                if (Active)
                {
                    return !pawn.Deathresting;
                }
                return false;
            }
        }


        private bool _IsLocked = false;
        public bool IsLocked
        {
            get
            {
                return _IsLocked;
            }
            set => _IsLocked = value;
        }

        private bool _IsRegenEnabled = false;
        public bool IsRegenEnabled
        {
            get
            {
                return _IsRegenEnabled;
            }
            set => _IsRegenEnabled = value;
        }

        private int CurrentRegenTick = 0;

        public override float Value
        {
            get => base.Value;
            set => base.Value = Mathf.Clamp(value, 0f, Max);
        }

        public float ValueCostMultiplied => Value * CostMult;
        public string DisplayLabel => ResourceDef?.resourceName ?? "Unknown Resource";
        public float ResourceLossPerDay => def?.resourceLossPerDay ?? 0f;
        public override float InitialResourceMax => ResourceDef?.maxStat != null ? Pawn.GetStatValue(ResourceDef.maxStat, true, 1250) : 100f;
        public override float MinLevelForAlert => 0.15f;
        public override float MaxLevelOffset => 0.1f;

        protected float defaultMax = 10f;

        private float lastMax;
        public override float Max
        {
            get
            {
                if (ResourceDef?.maxStat == null)
                    return defaultMax;

                float currentMax = Pawn.GetStatValue(ResourceDef.maxStat, true, 1250);
                if (currentMax != lastMax)
                {
                    lastMax = currentMax;
                    ForceBaseMaxUpdate(currentMax);
                }
                return currentMax;
            }
        }

        protected override Color BarColor => ResourceDef?.barColor ?? new ColorInt(3, 3, 138).ToColor;
        protected override Color BarHighlightColor => new ColorInt(42, 42, 145).ToColor;

        public override int ValueForDisplay => Mathf.RoundToInt(Value);
        public override int MaxForDisplay => Mathf.RoundToInt(Max);

        public float RegenAmount => ResourceDef?.regenStat != null ? Pawn.GetStatValue(ResourceDef.regenStat, true, 100) : 1f;
        public float RegenSpeed => ResourceDef?.regenSpeedStat != null ? Pawn.GetStatValue(ResourceDef.regenSpeedStat, true, 100) : 1f;
        public int RegenTicks => ResourceDef?.regenTicks != null ? Mathf.RoundToInt(Pawn.GetStatValue(ResourceDef.regenTicks, true, 100) * RegenSpeed) : Mathf.RoundToInt(2500 * RegenSpeed);
        public float CostMult => ResourceDef?.costMult != null ? Pawn.GetStatValue(ResourceDef.costMult, true, 100) : 1f;

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

        public void Consume(float Amount, bool addToUsedTotal = true)
        {
            if (!ModsConfig.BiotechActive)
                return;

            if (IsLocked)
            {
                return;
            }

            if(addToUsedTotal) TotalResourceUsed += Amount;
            Value -= Amount * CostMult;
        }

        public void Restore(float Amount)
        {
            if (!ModsConfig.BiotechActive)
                return;

            if (IsLocked)
            {
                return;
            }

            Value += Amount;
        }

        public bool Has(float Amount)
        {
            if (!ModsConfig.BiotechActive)
                return false;

            if (ResourceIsUnavailable(out string reason))
            {
                return false;
            }

            return Value >= Amount * CostMult;
        }

        public override void Tick()
        {
            base.Tick();
            if (IsRegenEnabled)
            {
                CurrentRegenTick++;
                if (CurrentRegenTick >= RegenTicks)
                {
                    Restore(RegenAmount);
                    ResetRegenTicks();
                }
            }
        }

        public void ResetRegenTicks()
        {
            CurrentRegenTick = 0;
        }

        public override void SetTargetValuePct(float val)
        {
            targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
        }

        public bool ResourceIsUnavailable(out string reason)
        {
            if (IsLocked)
            {
                reason = "Resource Locked";
                return true;
            }

            if (!EnableResource)
            {
                reason = "Resource not enabled";
                return true;
            }

            reason = string.Empty;
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }


            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = $"Add 50 {ResourceLabel}",
                    defaultDesc = $"Add 50 {ResourceLabel}",
                    action = () =>
                    {
                        this.Restore(50f);
                    }
                };

                yield return new Command_Action()
                {
                    defaultLabel = $"Remove 50 {ResourceLabel}",
                    defaultDesc = $"Remove 50 {ResourceLabel}",
                    action = () =>
                    {
                        this.Consume(50f, false);
                    }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableResource, "resourceEnabled", defaultValue: true);
            Scribe_Values.Look(ref CurrentRegenTick, "currentRegenTick", defaultValue: 0);
            Scribe_Values.Look(ref TotalResourceUsed, "TotalResourceUsed", defaultValue: 0);
        }
    }
}
