using RimWorld;
using Verse;
using static Verse.DamageWorker;

namespace MagicAndMyths
{
    // Example comp: Spawn filth on damage
    public class DamageWorkerCompProperties_SpawnFilth : DamageWorkerCompProperties
    {
        public ThingDef filthDef;
        public IntRange filthCount = new IntRange(1, 3);
        public float radius = 1f;

        public DamageWorkerCompProperties_SpawnFilth()
        {
            compClass = typeof(DamageWorkerComp_SpawnFilth);
        }
    }

    public class DamageWorkerComp_SpawnFilth : DamageWorkerComp
    {
        DamageWorkerCompProperties_SpawnFilth Props => (DamageWorkerCompProperties_SpawnFilth)props;

        public override DamageResult PostApply(DamageInfo dinfo, Thing thing, DamageResult result)
        {
            if (thing.Spawned && Props.filthDef != null)
            {
                int count = Props.filthCount.RandomInRange;
                for (int i = 0; i < count; i++)
                {
                    IntVec3 pos = thing.Position + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(Props.radius))];
                    if (pos.InBounds(thing.Map) && pos.Walkable(thing.Map))
                    {
                        FilthMaker.TryMakeFilth(pos, thing.Map, Props.filthDef);
                    }
                }
            }
            return result;
        }
    }
}