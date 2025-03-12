using Verse;

namespace MagicAndMyths
{
    public static class SquadUtil
    {
        public static bool TryGetSquadLeader(this Pawn pawn, out ISquadLeader SquadLeader)
        {
            bool result = false;
            SquadLeader = null;

            foreach (var item in pawn.health.hediffSet.hediffs)
            {
                if (item is ISquadLeader squadLeader)
                {
                    SquadLeader = squadLeader;
                    return true;
                }
            }

            return result;
        }

        public static bool IsPartOfSquad(this Pawn pawn, out ISquadMember SquadLeader)
        {
            bool result = false;
            SquadLeader = null;

            foreach (var item in pawn.health.hediffSet.hediffs)
            {

                if (item is ISquadMember squadLeader)
                {
                    SquadLeader = squadLeader;
                    return true;
                }
            }

            return result;
        }
    }
}
