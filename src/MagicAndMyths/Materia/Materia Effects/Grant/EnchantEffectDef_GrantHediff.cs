using RimWorld;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_GrantHediff : EnchantEffectDef
    {
        public HediffDef hediff;
        public bool overwriteExisting = true;
        public bool removeOnUnEquip = true;

        public EnchantEffectDef_GrantHediff()
        {
            workerClass = typeof(EnchantEffect_GrantHediff);
        }

        public override string EffectDescription => $"Grants the {hediff.LabelCap} status while equipped";


    }

    public class EnchantEffect_GrantHediff : EnchantWorker
    {
        EnchantEffectDef_GrantHediff Def => (EnchantEffectDef_GrantHediff)def;

        private Hediff hediffRef;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);


            if (Def.hediff != null && !EquippingPawn.health.hediffSet.HasHediff(Def.hediff))
            {
                hediffRef = EquippingPawn.health.AddHediff(Def.hediff);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {


            if (hediffRef != null && EquippingPawn.health.hediffSet.HasHediff(hediffRef.def))
            {
                EquippingPawn.health.RemoveHediff(hediffRef);
                hediffRef = null;
            }
            base.Notify_Unequipped(pawn);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref hediffRef, "hediffRef");
        }
    }
}