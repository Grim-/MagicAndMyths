using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobGiver_StandAndStarePeriodic : JobGiver_StandAndStare
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = base.TryGiveJob(pawn);
            job.expiryInterval = 1250;
            return job;
        }
    }
}
