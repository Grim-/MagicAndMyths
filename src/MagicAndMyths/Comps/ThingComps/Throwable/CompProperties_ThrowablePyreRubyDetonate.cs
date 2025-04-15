using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowablePyreRubyDetonate : CompProperties_Throwable
    {
        public CompProperties_ThrowablePyreRubyDetonate()
        {
            compClass = typeof(Comp_ThrowablePyreRubyDetonate);
        }
    }

    public class Comp_ThrowablePyreRubyDetonate : Comp_Throwable
    {
        public override void OnLanded(IntVec3 position, Map map, Pawn throwingPawn)
        {
            base.OnLanded(position, map, throwingPawn);


            if (this.parent.TryGetComp<Comp_PyreRuby>(out Comp_PyreRuby pyreRuby))
            {
                pyreRuby.ReleaseStoredFires();
            }
        }
    }
}