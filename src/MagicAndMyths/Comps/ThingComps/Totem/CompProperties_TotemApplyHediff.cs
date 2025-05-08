using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{



    public class CompProperties_TotemApplyHediff : CompProperties_BaseTotem
    {
        public HediffDef hediff;
        public FloatRange severity = new FloatRange(1f, 1f);
        public bool removeOnLeaveRadius = true;
        public EffecterDef onTickEffect = null;
        public CompProperties_TotemApplyHediff()
        {
            compClass = typeof(Comp_TotemApplyHediff);
        }
    }

    public class Comp_TotemApplyHediff : Comp_BaseTotem
    {
        protected CompProperties_TotemApplyHediff Props => (CompProperties_TotemApplyHediff)props;

        private HashSet<Pawn> previousTargets = new HashSet<Pawn>();

        public override void OnTotemTick()
        {
            base.OnTotemTick();

            if (previousTargets.Count > 0)
            {
                if (Props.removeOnLeaveRadius)
                {
                    foreach (var previousTarget in previousTargets)
                    {
                        if (previousTarget.health.hediffSet.HasHediff(Props.hediff))
                        {
                            previousTarget.health.RemoveHediff(previousTarget.health.hediffSet.GetFirstHediffOfDef(Props.hediff));
                        }
                    }
                }
                previousTargets.Clear();
            }

            List<Pawn> pawnsInRange = GetPawnsInRange();

            Log.Message($"Targets found {pawnsInRange.Count}");

            foreach (var item in pawnsInRange)
            {
                if (!item.health.hediffSet.HasHediff(Props.hediff))
                {
                    item.health.AddHediff(Props.hediff);

                    if (Props.onTickEffect != null)
                    {
                        Props.onTickEffect.Spawn(item.Position, item.Map, 1);
                    }
                }


                if (Props.removeOnLeaveRadius)
                {
                    previousTargets.Add(item);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref previousTargets, "previousTargets", LookMode.Reference);
        }
    }
}
