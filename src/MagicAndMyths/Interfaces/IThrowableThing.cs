using Verse;

namespace MagicAndMyths
{
    public interface IThrowableThing
    {
        bool IsThrowableAtAll { get; }


        DamageDef ImpactDamageType { get; }


        void OnThrown(IntVec3 position, Map map, Pawn throwingPawn = null);
        void OnLanded(IntVec3 position, Map map, Pawn throwingPawn = null);
        void OnImpactedThing(IntVec3 position, Map map, Pawn throwingPawn = null,Thing impactedThing = null);
    }
}