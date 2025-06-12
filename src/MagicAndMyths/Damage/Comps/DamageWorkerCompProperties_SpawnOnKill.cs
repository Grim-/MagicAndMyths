using Verse;
using static Verse.DamageWorker;

namespace MagicAndMyths
{
    // Example comp: Spawn things on death (OnKilled)
    public class DamageWorkerCompProperties_SpawnOnKill : DamageWorkerCompProperties
    {
        public ThingDef thingToSpawn;
        public IntRange spawnCount = new IntRange(1, 1);
        public float spawnChance = 1f;
        public bool onlyPawns = true;

        public DamageWorkerCompProperties_SpawnOnKill()
        {
            compClass = typeof(DamageWorkerComp_SpawnOnKill);
        }
    }

    public class DamageWorkerComp_SpawnOnKill : DamageWorkerComp
    {
        DamageWorkerCompProperties_SpawnOnKill Props => (DamageWorkerCompProperties_SpawnOnKill)props;

        public override bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            if (Props.onlyPawns && !(thing is Pawn)) return false;
            return Rand.Chance(Props.spawnChance);
        }

        public override void OnKilled(DamageInfo dinfo, Thing thing, DamageResult result)
        {
            if (thing.Spawned && Props.thingToSpawn != null)
            {
                int count = Props.spawnCount.RandomInRange;
                for (int i = 0; i < count; i++)
                {
                    Thing spawned = ThingMaker.MakeThing(Props.thingToSpawn);
                    GenPlace.TryPlaceThing(spawned, thing.Position, thing.Map, ThingPlaceMode.Near);
                }
            }
        }
    }
}