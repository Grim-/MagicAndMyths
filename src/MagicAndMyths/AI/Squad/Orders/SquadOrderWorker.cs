using Verse;

namespace MagicAndMyths
{
    public abstract class SquadOrderWorker
    {
        public ISquadLeader SquadLeader;
        public ISquadMember SquadMember;

        public SquadOrderSettings SquadOrderSettings;
        public abstract bool CanExecuteOrder(LocalTargetInfo Target);

        public abstract void ExecuteOrder(LocalTargetInfo Target);
    }
}
