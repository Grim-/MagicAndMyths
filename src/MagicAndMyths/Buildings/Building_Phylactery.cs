using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class Building_Phylactery : Building
    {

        private Pawn referencedPawn = null;
        protected bool IsBoundToPawn => referencedPawn != null;
        private int respawnTicks = 200;

        private int respawnTickTimer = 0;

        private bool IsRessurrecting = false;

        private int ressurrectionCounter = 0;

        public void SetPawn(Pawn newPawn)
        {
            if (IsBoundToPawn)
            {
                return;
            }

            referencedPawn = newPawn;
            Hediff_Phylactery _Phylactery = (Hediff_Phylactery)referencedPawn.health.GetOrAddHediff(MagicAndMythDefOf.MagicAndMyths_Phylactery);
            _Phylactery.SetBuildingReference(this);
        }



        private void StartRess()
        {
            if (IsRessurrecting)
            {
                return;
            }

            IsRessurrecting = true;
            respawnTickTimer = 0;
        }

        public override void Tick()
        {
            base.Tick();

            if (this.Spawned && this.IsRessurrecting)
            {

                respawnTickTimer++;

                if (respawnTickTimer >= respawnTicks)
                {
                    RespawnAtPhylactery();
                }

            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {

            if (referencedPawn == null && !selPawn.Dead)
            {
                yield return new FloatMenuOption($"Bind soul to", () =>
                {
                    SetPawn(selPawn);
                });
            }
        }

        public void Notify_BoundPawnDied()
        {
            StartRess();
           //RespawnAtPhylactery();
        }


        private void RespawnAtPhylactery()
        {
            if (referencedPawn != null)
            {
                if (referencedPawn.Corpse != null)
                {
                    if (ResurrectionUtility.TryResurrect(referencedPawn))
                    {
                        referencedPawn.Position = this.Position;
                        referencedPawn.Notify_Teleported(true, true);
                        ressurrectionCounter++;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref IsRessurrecting, "IsRessurrecting");
            Scribe_Values.Look(ref respawnTickTimer, "respawnTickTimer");
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
        }
    }
}
