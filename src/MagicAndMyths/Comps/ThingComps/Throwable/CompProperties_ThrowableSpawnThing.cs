using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableSpawnThing : CompProperties_Throwable
    {
        public ThingDef thingToSpawn;
        public IntRange spawnCount = new IntRange(1, 1);
        public bool inheritStuff = false;
        public bool inheritQuality = false;
        public bool randomRotation = true;
        public bool destroyOnImpact = true;

        public CompProperties_ThrowableSpawnThing()
        {
            compClass = typeof(Comp_ThrowableSpawnThing);
        }
    }

    public class Comp_ThrowableSpawnThing : Comp_Throwable
    {
        CompProperties_ThrowableSpawnThing Props => (CompProperties_ThrowableSpawnThing)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            if (Props.thingToSpawn != null)
            {
                int numToSpawn = Props.spawnCount.RandomInRange;
                for (int i = 0; i < numToSpawn; i++)
                {
                    Thing newThing = ThingMaker.MakeThing(Props.thingToSpawn,
                        Props.inheritStuff ? this.parent.Stuff : null);

                    if (Props.inheritQuality && newThing.TryGetComp<CompQuality>() != null
                        && this.parent.TryGetComp<CompQuality>() != null)
                    {
                        newThing.TryGetComp<CompQuality>().SetQuality(
                            this.parent.TryGetComp<CompQuality>().Quality, ArtGenerationContext.Colony);
                    }

                    if (Props.randomRotation)
                    {
                        newThing.Rotation = Rot4.Random;
                    }

                    GenSpawn.Spawn(newThing, position, map);
                }
            }

            if (Props.destroyOnImpact)
            {
                this.parent.Destroy();
            }
        }
    }
}