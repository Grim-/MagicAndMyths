using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_PhoenixDown : HediffCompProperties
    {
        public int ressurrections = 1;

        public HediffCompProperties_PhoenixDown()
        {
            compClass = typeof(HediffComp_PhoenixDown);
        }
    }

    public class HediffComp_PhoenixDown : HediffComp
    {
        HediffCompProperties_PhoenixDown Props => (HediffCompProperties_PhoenixDown)props;

        private int ressCounter = 0;


        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            if (ressCounter < Props.ressurrections)
            {
                ressCounter++;

                if (this.Pawn.Corpse != null)
                {

                    if (ResurrectionUtility.TryResurrectWithSideEffects(this.Pawn))
                    {
                        if (ressCounter >= Props.ressurrections)
                        {
                            this.parent.Severity = 0;

                            //if (this.MateriaSlot != null)
                            //{
                            //    this.MateriaSlot.EnchantComp.UnequipMateria(MateriaSlot, false);
                            //}
                        }

                        Messages.Message("Phoenix Down revived : " + this.Pawn.LabelCap, this.Pawn, MessageTypeDefOf.PositiveEvent);
                    }

                }

            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Values.Look(ref ressCounter, "ressCounter");
        }
    }
}
