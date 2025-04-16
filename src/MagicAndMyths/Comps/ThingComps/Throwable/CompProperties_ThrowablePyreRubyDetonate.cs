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
        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            if (this.parent.TryGetComp(out Comp_PyreRuby pyreRuby))
            {
                Log.Message("Pyreruby detonate");
                pyreRuby.ReleaseStoredFires();
            }
        }
    }
}