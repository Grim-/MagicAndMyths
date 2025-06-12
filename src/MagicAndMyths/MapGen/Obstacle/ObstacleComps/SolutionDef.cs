using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class SolutionDef : Def
    {
        public int difficultyLevel = 5;
        public StatDef relevantStat;
        public SkillDef relevantSkill;
        public PawnCapacityDef relevantCapacity;
        public JobDef jobDef;
        public int workTicks = 100;
    }
}
