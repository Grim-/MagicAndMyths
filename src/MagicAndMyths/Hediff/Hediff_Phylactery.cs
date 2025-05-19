using Verse;

namespace MagicAndMyths
{


    public class Hediff_Phylactery : HediffWithComps
    {
        private Building_Phylactery Phylactery;

        public void SetBuildingReference(Building_Phylactery building)
        {
            Phylactery = building;
        }



        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            if (Phylactery != null)
            {
                Phylactery.Notify_BoundPawnDied();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();


            Scribe_References.Look(ref Phylactery, "Phylactery");
        }
    }
}
