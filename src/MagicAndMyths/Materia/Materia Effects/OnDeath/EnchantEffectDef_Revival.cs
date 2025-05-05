using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_Revival : EnchantEffectDef
    {
        public float reviveHealthPercentage = 0.3f;
        public SoundDef reviveSound;

        public EnchantEffectDef_Revival()
        {
            workerClass = typeof(EnchantEffect_Revival);
        }
    }

    public class EnchantEffect_Revival : EnchantWorker
    {
        EnchantEffectDef_Revival Def => (EnchantEffectDef_Revival)def;

        public override void Notify_OwnerKilled()
        {
            if (this.EquippingPawn != null && this.EquippingPawn.Corpse != null && this.EquippingPawn.Corpse.InnerPawn != null)
            {
                //Log.Message("Notify_OwnerKilled");
                if (ResurrectionUtility.TryResurrect(this.EquippingPawn.Corpse.InnerPawn))
                {
                    Messages.Message($"{this.ParentEquipment.Label}  Revived {this.EquippingPawn.Label} and shattered.", MessageTypeDefOf.PositiveEvent);
                    DestroyParentMateria();
                }
            }


        }
    }
}