using RimWorld;
using SquadBehaviour;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Gene_DeathKnight : Gene_Class, IThingHolder
    {
        #region Fields and Properties
        public int WillStat => Mathf.CeilToInt(this.pawn.GetStatValue(MagicAndMythDefOf.MagicAndMyths_Will, true, 1200));

        public float WillCapacityAsPercent => (float)WillRequiredForUndead / (float)WillStat;

        private ThingOwner storedCreature;

        public int WillRequiredForUndead
        {
            get
            {
                int willTotal = 0;
                foreach (var item in SquadLeaderComp.AllSquadsPawns)
                {
                    //pull from def later
                    willTotal += 1;
                }

                return willTotal;
            }
        }



        public Comp_PawnSquadLeader SquadLeaderComp => this.pawn.GetComp<Comp_PawnSquadLeader>();

        public bool IsOverWillLimits => WillRequiredForUndead > this.WillStat;
        public IThingHolder ParentHolder => this.pawn.ParentHolder;
        #endregion


        protected int FixedSquadID = -666;


        public Gene_DeathKnight()
        {
            storedCreature = new ThingOwner<Pawn>(this, false, LookMode.Deep);
        }


        public override void PostAdd()
        {
            base.PostAdd();

            if (this.pawn.TryGetComp(out Comp_PawnSquadLeader pawnSquadLeader))
            {
                pawnSquadLeader.SetSquadLeader(true);
                if (!pawnSquadLeader.HasAnySquad())
                {
                    pawnSquadLeader.AddSquad(666);
                    pawnSquadLeader.GetSquadByID(FixedSquadID).squadName = "Legion of the damned";
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.pawn.health.Dead && this.pawn.IsHashIntervalTick(2400))
            {
                CheckWillLimit();
            }
        }

        private void CheckWillLimit()
        {
            if (WillRequiredForUndead > this.WillStat)
            {
                //pick pawns until under limit, turn them feral
            }
        }


        #region IThingHolder Implementation
        public ThingOwner GetDirectlyHeldThings()
        {
            // Only return the stored creatures
            return storedCreature;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }
        #endregion


        public void SetupCreature(Pawn pawn)
        {
            if (!pawn.TryGetComp(out Comp_PawnSquadMember squadMember))
            {
                Log.Message($"Cannot set up creature {pawn.Label} it has no squad member comp");
                return;
            }

            if (SquadLeaderComp == null)
            {
                Log.Message($"Cannot set up creature {pawn.Label} it's master {this.pawn.Label} has no squad leader comp");
                return;
            }


            Log.Message($"{pawn.Label} setting squad leader to {this.pawn.Label}");


            squadMember.SetSquadLeader(this.pawn);



            if (SquadLeaderComp.TryGetSquadByID(FixedSquadID, out Squad legionSquad))
            {
                Log.Message($"{this.pawn.Label} adding {pawn.Label} to squad");
                legionSquad.AddMember(pawn);
            }


            Log.Message($"Assigned Squad : {squadMember.AssignedSquad}");

            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }

            DraftingUtility.MakeDraftable(pawn);


            if (pawn.RaceProps.Humanlike)
            {
                //pawn.guest.Recruitable = true;
                ////pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Slave);
                //pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }
            else
            {
                MagicUtil.TrainPawn(pawn, this.pawn);
            }

            if (pawn.playerSettings != null)
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            }
        }

        /// <summary>
        /// Summons a stored creature to the specified position.
        /// Removes it from stored list and adds to active list.
        /// </summary>
        /// <param name="pawn">The pawn to summon</param>
        /// <param name="position">The position to summon at</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SummonCreature(Pawn pawn, IntVec3 position)
        {
            storedCreature.RemoveAll(x => x == pawn);

            if (SquadLeaderComp.TryGetSquadByID(FixedSquadID, out Squad legionSquad))
            {
                Log.Message($"{this.pawn.Label} adding {pawn.Label} to squad");
                legionSquad.AddMember(pawn);

                Pawn summonedPawn = pawn;
                SetupCreature(summonedPawn);

                if (!summonedPawn.Spawned)
                {
                    GenSpawn.Spawn(summonedPawn, position, this.pawn.Map);
                }

                if (summonedPawn.abilities == null)
                {
                    summonedPawn.abilities = new Pawn_AbilityTracker(summonedPawn);
                }

                Log.Message($"Successfully summoned creature {pawn.Label}");
                return true;
            }
            else
            {
                Log.Message($"Failed to add {pawn.Label} to {this.pawn.Label} squad.");
                return false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look<ThingOwner>(ref storedCreature, "storedDeadMinions", new object[] { this });
        }
    }
}