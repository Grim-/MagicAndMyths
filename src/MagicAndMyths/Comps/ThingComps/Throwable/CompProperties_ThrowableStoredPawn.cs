using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableStoredPawn : CompProperties_Throwable
    {
        public CompProperties_ThrowableStoredPawn()
        {
            compClass = typeof(Comp_ThrowableStoredPawn);
        }
    }

    public class Comp_ThrowableStoredPawn : Comp_Throwable
    {
        public CompProperties_ThrowableStoredPawn Props => (CompProperties_ThrowableStoredPawn)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            Pawn singlePawn = position.GetFirstPawn(map);
            Comp_PawnStorage pawnStorage = this.parent.GetComp<Comp_PawnStorage>();
            if (pawnStorage != null)
            {
                if (pawnStorage.HasStored)
                {
                    //release
                    pawnStorage.ReleasePawn(position, map);
                }
                else
                {
                    if (singlePawn != null)
                    {
                        pawnStorage.StorePawn(singlePawn);
                    }
                }
            }


        }
    }
}