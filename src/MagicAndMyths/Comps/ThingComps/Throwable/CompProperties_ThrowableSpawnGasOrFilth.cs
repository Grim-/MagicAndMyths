using RimWorld;
using Verse;

namespace MagicAndMyths
{

    public class CompProperties_ThrowableSpawnGasOrFilth : CompProperties_Throwable
    {
        public ThingDef filthDef;
        public IntRange filthCount = new IntRange(5, 10);
        public bool gasSpawn = false;
        public GasType gasType = GasType.BlindSmoke;
        public bool destroyOnImpact = true;

        public CompProperties_ThrowableSpawnGasOrFilth()
        {
            compClass = typeof(Comp_ThrowableSpawnGasOrFilth);
        }
    }

    public class Comp_ThrowableSpawnGasOrFilth : Comp_Throwable
    {
        CompProperties_ThrowableSpawnGasOrFilth Props => (CompProperties_ThrowableSpawnGasOrFilth)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            if (Props.gasSpawn)
            {
                GasUtility.AddGas(position, map, Props.gasType, Props.radius);
            }
           
            if (Props.filthDef != null)
            {
                int numFilthToSpawn = Props.filthCount.RandomInRange;
                for (int i = 0; i < numFilthToSpawn; i++)
                {
                    IntVec3 randomCell = position + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(Props.radius))];
                    if (randomCell.InBounds(map) && randomCell.Walkable(map))
                    {
                        FilthMaker.TryMakeFilth(randomCell, map, Props.filthDef);
                    }
                }
            }

            if (Props.destroyOnImpact)
            {
                this.parent.Destroy();
            }
        }
    }

}