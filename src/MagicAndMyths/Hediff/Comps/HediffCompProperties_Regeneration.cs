using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_Regeneration : HediffCompProperties
    {
        public float healPerRegenTick = 0.1f;
        public int disableTicks = 6000;
        public bool canCurePermanent = false;
        public bool canCureAddiction = false;

        public List<DamageDef> disruptingDamageTypes;

        public HediffCompProperties_Regeneration()
        {
            compClass = typeof(HediffComp_Regeneration);
        }
    }

    public class HediffComp_Regeneration : HediffComp
    {
        HediffCompProperties_Regeneration Props => (HediffCompProperties_Regeneration)props;

        protected int RegenDisabledTick = -1;
        protected int regenTick = 0;
        protected bool CanRegen = true;

        protected bool CanStartRegenAgain => !CanRegen && !MagicUtil.HasCooldownByTick(RegenDisabledTick, Props.disableTicks);

        protected Hediff CurrenTarget = null;

        public override string CompDescriptionExtra
        {
            get
            {
                string description = base.CompDescriptionExtra;
                string regenBreakTypes = string.Join(", ", Props.disruptingDamageTypes.Select(x => x.label));

                description += $"These damage types disable regen [{regenBreakTypes}] for {GenDate.ToStringTicksToPeriod(Props.disableTicks)}.";

                if (!CanRegen)
                {
                    description += $"\r\n<color=red>Regeneration disabled</color> {GenDate.ToStringTicksToPeriod(DisabledTicksRemaining)} remaining.";
                }
                return description;
            }
        }


        protected int DisabledTicksRemaining => RegenDisabledTick + Props.disableTicks - Current.Game.tickManager.TicksGame;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (CanRegen)
            {
                if (this.Pawn.IsHashIntervalTick(10))
                {
                    if (CurrenTarget == null)
                    {
                        if (MagicUtil.TryGetWorstInjury(this.Pawn, out Hediff foundHediff, out BodyPartRecord partRecord, (Hediff hediff) =>
                        {
                            return !hediff.IsPermanent() && hediff.def.isBad && !hediff.def.IsAddiction;
                        }))
                        {
                            CurrenTarget = foundHediff;
                        }
                    }
                    else
                    {
                        //Log.Message($"regenerating {CurrenTarget.Label}");
                        CurrenTarget.Severity -= 0.1f;

                        if (CurrenTarget.Severity <= 0)
                        {
                            CurrenTarget = null;
                        }
                    }
                }
            }
            else
            {
                if (CanStartRegenAgain)
                {
                    EnableRegen();
                }
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (Props.disruptingDamageTypes != null)
            {
                if (Props.disruptingDamageTypes.Contains(dinfo.Def))
                {
                    DisableRegen();
                }
            }
        }


        public virtual void EnableRegen()
        {
            CanRegen = true;
        }

        public virtual void DisableRegen()
        {
            RegenDisabledTick = Current.Game.tickManager.TicksGame;
            CanRegen = false;

            MoteMaker.ThrowText(this.Pawn.DrawPos, this.Pawn.Map, $"Regeneration broke!", 3);
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref RegenDisabledTick, "RegenDisabledTick");
            Scribe_Values.Look(ref regenTick, "regenTick");
            Scribe_Values.Look(ref CanRegen, "CanRegen");
        }
    }
}