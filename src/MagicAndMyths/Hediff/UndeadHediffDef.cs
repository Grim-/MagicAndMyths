using Verse;

namespace MagicAndMyths
{
    public class UndeadHediffDef : HediffDef
    {
        public int regenTicks = 1250;
        public float baseHealAmount = 15f;


        public UndeadHediffDef()
        {
            hediffClass = typeof(Hediff_Undead);
        }
    }
}
