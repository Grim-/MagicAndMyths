using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class UndeadDef : Def
    {
        public int baseWillCost = 1;
        public float willCostMultiplier = 1f;
        public HediffDef hediff;
        public PawnKindDef kind;


        public List<AbilityDef> abilitiesToGain;

        public List<BackstoryDef> childhoodBackstories;
        public List<BackstoryDef> adulthoodBackstories;
    }
}
