using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_ModifyNeedInterval : EnchantEffectDef
    {
        public NeedDef needDef;
        public int intervalTicks = 1250;
        public FloatRange amount = new FloatRange(0.01f, 0.04f);

        public EnchantEffectDef_ModifyNeedInterval()
        {
            workerClass = typeof(EnchantEffect_ModifyNeedInterval);
        }

        public override string EffectDescription => $"Fulfills {needDef.LabelCap} by {amount.Average} per {intervalTicks.TicksToSeconds()}";
    }

    public class EnchantEffect_ModifyNeedInterval : EnchantWorker
    {
        EnchantEffectDef_ModifyNeedInterval Def => (EnchantEffectDef_ModifyNeedInterval)def;
        protected int ticks = 0;

        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);
            ticks++;

            if (ticks >= Def.intervalTicks)
            {
                if (pawn.needs != null)
                {
                    Need need = pawn.needs.TryGetNeed(Def.needDef);
                    if (need != null)
                    {
                        need.CurLevel += Def.amount.RandomInRange;
                    }
                }

                ticks = 0;
            }
        }
    }
}