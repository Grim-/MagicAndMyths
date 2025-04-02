using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class CompMechanism : ThingComp
    {
        // Reference to parent obstacle
        protected Obstacle parentObstacle;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parentObstacle = (Obstacle)parent;
        }

        public abstract void OnSolutionComplete();

        public virtual void OnProgress(float progressPercent)
        {
          
        }

        public virtual void OnSolutionFailed()
        {
         
        }
    }


}
