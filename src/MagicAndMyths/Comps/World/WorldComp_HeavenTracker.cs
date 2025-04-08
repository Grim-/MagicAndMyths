using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class WorldComp_HeavenTracker : WorldComponent
    {
        private HashSet<DeadColonistRecord> deadColonists = new HashSet<DeadColonistRecord>();

        public WorldComp_HeavenTracker(World world) : base(world)
        {
            EventManager.OnThingKilled += TrackDeadColonist;
        }

        private void TrackDeadColonist(Pawn pawn, DamageInfo info, Hediff culprit)
        {
            if (pawn == null || !pawn.IsColonist || deadColonists.Any(x=> x.Pawn == pawn))
                return;

            DeadColonistRecord record = new DeadColonistRecord
            {
                Pawn = pawn,
                PawnCorpse = pawn.Corpse,
                DeathTick = Find.TickManager.TicksGame,
            };

            deadColonists.Add(record);
            Log.Message($"Colonist {record.Pawn.Name} added to heaven registry at tick {record.DeathTick}.");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref deadColonists, "deadColonists", LookMode.Deep);
        }

        // Returns the list of dead colonists for external use
        public List<DeadColonistRecord> GetDeadColonists()
        {
            return deadColonists.ToList();
        }
    }


    public class DeadColonistRecord : IExposable
    {
        public Pawn Pawn;
        public Corpse PawnCorpse;
        public int DeathTick;

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "Pawn");
            Scribe_References.Look(ref PawnCorpse, "PawnCorpse");

            Scribe_Values.Look(ref DeathTick, "deathTick");
        }
    }
}
