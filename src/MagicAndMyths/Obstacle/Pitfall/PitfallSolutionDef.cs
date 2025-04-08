using System;

namespace MagicAndMyths
{
    public class PitfallSolutionDef : SolutionDef
    {
        public Type workerClass;

        public PitfallSolutionWorker CreateWorker(Building_PitfallTile pitfallTile)
        {
            PitfallSolutionWorker jumpWorker = (PitfallSolutionWorker)Activator.CreateInstance(workerClass);
            jumpWorker.parent = pitfallTile;
            jumpWorker.def = this;
            jumpWorker.PitfallTile = pitfallTile;
            return jumpWorker;
        }
    }
}
