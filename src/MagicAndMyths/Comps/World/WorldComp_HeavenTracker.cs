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
            EventManager.Instance.OnThingKilled += TrackDeadColonist;
        }

        private void TrackDeadColonist(Pawn pawn, DamageInfo info, Hediff culprit)
        {
            if (pawn == null || !pawn.IsColonist || deadColonists.Any(x=> x.Pawn == pawn))
                return;

            try
            {
                DeadColonistRecord record = new DeadColonistRecord
                {
                    Pawn = pawn,
                    PawnCorpse = pawn.Corpse,
                    DeathTick = Find.TickManager.TicksGame,
                    DeathReason = culprit.combatLogText,
                    Killer = info.InstigatorGuilty && info.Instigator != null ? info.Instigator : null
                };

                deadColonists.Add(record);
                //Log.Message($"Colonist {record.Pawn.Name} added to heaven registry at tick {record.DeathTick}.");
            }
            catch (System.Exception e)
            {
                Log.Message(e);
            }
        }

        public void UnTrackColonist(Pawn pawn)
        {
            if (deadColonists.Any(x=> x.Pawn == pawn))
            {
                deadColonists.RemoveWhere(x => x.Pawn == pawn);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref deadColonists, "deadColonists", LookMode.Deep);
        }

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
        public string DeathReason = "";
        public Thing Killer = null;

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "Pawn");
            Scribe_References.Look(ref Killer, "Killer");
            Scribe_References.Look(ref PawnCorpse, "PawnCorpse");
            Scribe_Values.Look(ref DeathReason, "deathReason");
            Scribe_Values.Look(ref DeathTick, "deathTick");
        }
    }
}
