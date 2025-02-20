using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class HediffComp_MagicTattooBase : HediffComp
    {
        public HediffCompProperties_MagicTattooBase Props => (HediffCompProperties_MagicTattooBase)props;


        protected TattooDef previousTattoo;
        protected TattooDef currentTattoo;
        protected int appliedTick = -1;
        protected bool hasApplied = false;


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ApplyTattoo(Props.tattooDef);
        }

        public virtual bool ApplyTattoo(TattooDef tattooDef)
        {
            if (tattooDef == null)
                return false;

            if (this.Pawn.style.FaceTattoo != null)
            {
                previousTattoo = this.Pawn.style.FaceTattoo;
            }

            this.Pawn.style.FaceTattoo = tattooDef;
            currentTattoo = tattooDef;
            appliedTick = Find.TickManager.TicksGame;
            hasApplied = true;
            return true;
        }

        public virtual bool RemoveTattoo()
        {
            this.Pawn.style.FaceTattoo = null;
            currentTattoo = null;
            hasApplied = false;
            return true;
        }


        public virtual void OnTattooApplied()
        {

        }

        public virtual void OnTattooRemoved()
        {

        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Defs.Look(ref currentTattoo, "currentTattoo");
            Scribe_Values.Look(ref appliedTick, "appliedTick");
            Scribe_Values.Look(ref hasApplied, "hasApplied");
        }

    }
}
