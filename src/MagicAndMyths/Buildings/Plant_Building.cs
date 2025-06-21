using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class Plant_Building : Plant
    {
        public override int YieldNow()
        {
            return 0;
        }

        public override void TickLong()
        {
            if (this.LifeStage == PlantLifeStage.Mature)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.WoodLog), this.Position, this.Map);

                if (!this.Destroyed)
                {
                    this.Destroy();
                }

            
                return;
            }

            base.TickLong();
        }
    }
}