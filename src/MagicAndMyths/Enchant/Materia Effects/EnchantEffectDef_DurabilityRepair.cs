using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_DurabilityRepair : EnchantEffectDef
    {
        public IntRange repairAmount = new IntRange(1, 2);
        public IntRange repairTicks = new IntRange(2400, 2400);
        public EnchantEffectDef_DurabilityRepair()
        {
            workerClass = typeof(EnchantEffect_DurabilityRepair);
        }
        public override string EffectDescription => $"Repairs {repairAmount.min} - {repairAmount.max} durability every {repairTicks.min} - {repairTicks.max} ticks.";
    }

    public class EnchantEffect_DurabilityRepair : EnchantWorker
    {
        EnchantEffectDef_DurabilityRepair Def => (EnchantEffectDef_DurabilityRepair)def;

        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);

            if (pawn.IsHashIntervalTick(2400))
            {
                if (this.ParentEquipment != null)
                {
                    this.ParentEquipment.HitPoints += Def.repairAmount.RandomInRange;
                }

            }
        }

    }
}