using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableSpawnFilth : CompProperties_Throwable
    {
        public ThingDef filthDef;
        public IntRange count = new IntRange(5, 10);

        public CompProperties_ThrowableSpawnFilth()
        {
            compClass = typeof(Comp_ThrowableSpawnFilth);
        }
    }

    public class Comp_ThrowableSpawnFilth : Comp_Throwable
    {
        CompProperties_ThrowableSpawnFilth Props => (CompProperties_ThrowableSpawnFilth)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            if (Props.filthDef != null)
            {


                List<IntVec3> CellsInRadis = GenRadial.RadialCellsAround(position, Props.radius, true).ToList();

                int numFilthToSpawn = Props.count.RandomInRange;
                for (int i = 0; i < numFilthToSpawn; i++)
                {
                    IntVec3 randomCell = CellsInRadis.RandomElement();
                    if (randomCell.InBounds(map) && randomCell.Walkable(map))
                    {
                        FilthMaker.TryMakeFilth(randomCell, map, Props.filthDef, 1, FilthSourceFlags.Unnatural);
                    }
                }
            }
        }
    }


}