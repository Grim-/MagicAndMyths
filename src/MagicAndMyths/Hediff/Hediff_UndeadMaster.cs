using RimWorld;
using RimWorld.Planet;
using SquadBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class Hediff_UndeadMaster : Hediff_SquadLeader, IThingHolder
    {
        #region Fields and Properties
        public int WillStat => Mathf.CeilToInt(this.pawn.GetStatValue(MagicAndMythDefOf.MagicAndMyths_Will, true, 1200));

        public float WillCapacityAsPercent => (float)WillRequiredForUndead / (float)WillStat;

        // Only store the unspawned pawns in a ThingOwner, if it fucking worked
        private ThingOwner storedCreature;

        public override string Description => base.Description + $"\r\nThis pawn has absorbed {storedCreature.Count} spirits.";

        public List<Pawn> AllActive => _ActiveSquads.SelectMany(x=> x.Value.Members.ToList()).ToList();

        public List<Pawn> AllStored => storedCreature.OfType<Pawn>().ToList();

        public int WillRequiredForUndead
        {
            get
            {
                int willTotal = 0;
                foreach (var item in AllSquadsPawns)
                {
                    //pull from def later
                    willTotal += 1;
                }

                return willTotal;
            }
        }
        public bool IsOverWillLimits => WillRequiredForUndead > this.WillStat;
        public IThingHolder ParentHolder => this.pawn.ParentHolder;
        #endregion

        // Constructor to initialize ThingOwner
        public Hediff_UndeadMaster()
        {
            storedCreature = new ThingOwner<Pawn>(this, false, LookMode.Deep);
            _ActiveSquads = new Dictionary<int, Squad>();
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
            if (pawn.health.hediffSet.TryGetHediff<Hediff_Undead>(out Hediff_Undead undead))
            {
                undead.SetSquadLeader(this.pawn);
            }
            else
            {
                Log.Message($"no undead hediff found for {pawn.Label}");
            }

            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }


            if (pawn.RaceProps.Humanlike)
            {
                //pawn.guest.Recruitable = true;
                ////pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Slave);
                //pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }
            else
            {
                SummonPatches.TrainPawn(pawn, this.pawn);
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
            // Check if already active
            if (IsPartOfAnySquad(pawn, out Squad activeSquad))
            {
                Log.Message($"Tried to Summon an already active creature");
                return false;
            }

            storedCreature.RemoveAll(x => x == pawn);

            if (AddToSquad(pawn))
            {
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

            Scribe_Deep.Look<ThingOwner>(ref storedCreature, "storedCursedSpirits", new object[] { this });
            Scribe_Collections.Look(ref _ActiveSquads, "activeSquads", LookMode.Value, LookMode.Deep);
            Scribe_Defs.Look(ref _FormationType, "formationType");
            Scribe_Values.Look(ref _FollowDistance, "followDistance", 5f);
            Scribe_Values.Look(ref InFormation, "inFormation", true);
        }
    }
}
