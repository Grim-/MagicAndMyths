using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableSpawnGas : CompProperties_Throwable
    {
        public IntRange count = new IntRange(5, 10);
        public GasType gasType = GasType.BlindSmoke;

        public CompProperties_ThrowableSpawnGas()
        {
            compClass = typeof(Comp_ThrowableSpawnGas);
        }
    }

    public class Comp_ThrowableSpawnGas : Comp_Throwable
    {
        CompProperties_ThrowableSpawnGas Props => (CompProperties_ThrowableSpawnGas)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);
            GasUtility.AddGas(position, map, Props.gasType, Props.radius);
        }
    }
}