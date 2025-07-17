using Verse;

namespace MagicAndMyths
{
    public class CompProperties_RaceBlinkDog : CompProperties
    {
        public CompProperties_RaceBlinkDog()
        {
            compClass = typeof(Comp_RaceBlinkDog);
        }
    }

    public class Comp_RaceBlinkDog : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            //if (this.parent is Pawn pawn)
            //{
            //    pawn.Graphic.color = Color.white;
            //    pawn.Graphic.colorTwo = Color.cyan;
            //}
        }
    }
}
