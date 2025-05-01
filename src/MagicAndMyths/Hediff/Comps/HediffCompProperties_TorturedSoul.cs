using RimWorld;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_TorturedSoul : HediffCompProperties
    {
        public HediffCompProperties_TorturedSoul()
        {
            compClass = typeof(HediffComp_TorturedSoul);
        }
    }


    public class HediffComp_TorturedSoul : HediffComp
    {
        protected Ideo targetIdeo = null;

        protected Ideo TargetIdeo
        {
            get
            {
                if (targetIdeo != null)
                {
                    return targetIdeo;
                }

                return Find.World.ideoManager.IdeosListForReading.FirstOrDefault(x => x.initialPlayerIdeo);
            }
        }

        public void SetTargetIdeo(Ideo newTarget)
        {
            targetIdeo = newTarget;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn.ideo != null)
            {
                Pawn.ideo.SetIdeo(TargetIdeo);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_References.Look(ref targetIdeo, "targetIdeo");
        }
    }
}