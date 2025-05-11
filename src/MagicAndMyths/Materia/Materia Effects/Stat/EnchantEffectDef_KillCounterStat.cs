using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_KillCounterStat : EnchantEffectDef_PawnStat
    {
        public float increasePerKill = 0.1f;

        public EnchantEffectDef_KillCounterStat()
        {
            workerClass = typeof(EnchantEffect_KillCounterStat);
        }

        public override string EffectDescription
        {
            get
            {
                string increaseText = modifierType == StatModifierType.Offset ?
                    $"+{increasePerKill}" :
                    $"x{increasePerKill}";

                return $"Increases {statToAffect.LabelCap} by {increaseText} for each enemy killed";
            }
        }
    }

    public class EnchantEffect_KillCounterStat : EnchantEffect_PawnStat
    {
        protected int killCounter = 0;
        protected EnchantEffectDef_KillCounterStat KillDef => (EnchantEffectDef_KillCounterStat)def;


        public override float GetStatOffset(StatDef stat)
        {
            if (stat == KillDef.statToAffect && KillDef.modifierType == StatModifierType.Offset)
            {
                return this.EquippingPawn.records.GetAsInt(RecordDefOf.KillsHumanlikes) * KillDef.increasePerKill;
            }
            return 0f;
        }

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == KillDef.statToAffect && KillDef.modifierType == StatModifierType.Factor)
            {
                return 1f + (this.EquippingPawn.records.GetAsInt(RecordDefOf.KillsHumanlikes) * KillDef.increasePerKill);
            }
            return 1f;
        }

        public override string GetExplanationString()
        {
            float currentBonus = killCounter * KillDef.increasePerKill;

            if (KillDef.modifierType == StatModifierType.Offset)
            {
                string sign = currentBonus >= 0 ? "+" : "";
                return $"{sign}{currentBonus:0.##} ({this.EquippingPawn.records.GetAsInt(RecordDefOf.KillsHumanlikes)} kills)";
            }
            else
            {
                return $"x{(1f + currentBonus):0.##} ({this.EquippingPawn.records.GetAsInt(RecordDefOf.KillsHumanlikes)} kills)";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref killCounter, "killCounter", 0);
        }
    }
}