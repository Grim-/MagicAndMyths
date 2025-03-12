using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{


    public class Hediff_UndeadMaster : HediffWithComps, IThingHolder, ISquadLeader
    {
        #region Fields and Properties

        //1 will = 1 undead
        private int undeadLimit => Mathf.CeilToInt(this.pawn.GetStatValue(MagicAndMythDefOf.MagicAndMyths_Will, true, 1200));

        // Only store the unspawned pawns in a ThingOwner, if it fucking worked
        private ThingOwner<Pawn> storedCreature;

        // Keep active creatures as a simple list since they're spawned on the map
        private HashSet<Pawn> activeCreature = new HashSet<Pawn>();

        private float _FollowDistance = 5f;
        public float FollowDistance => _FollowDistance;

        public bool InFormation = true;

        public bool ShowExtraOrders = true;

        private FormationUtils.FormationType _FormationType = FormationUtils.FormationType.Column;
        public FormationUtils.FormationType FormationType => _FormationType;

        public override string Description => base.Description + $"\r\nThis pawn has absorbed {storedCreature.Count} spirits.";


        private Lord SquadLord = null;

        public List<Pawn> AllSummons
        {
            get
            {
                // Use a HashSet to eliminate duplicates
                HashSet<Pawn> uniquePawns = new HashSet<Pawn>();
                foreach (var pawn in AllStored)
                    uniquePawns.Add(pawn);
                foreach (var pawn in AllActive)
                    uniquePawns.Add(pawn);
                return uniquePawns.ToList();
            }
        }

        public List<Pawn> AllActive => activeCreature.ToList();

        public List<Pawn> AllStored => storedCreature.InnerListForReading;

        public int WillRequiredForUndead
        {
            get
            {
                int willTotal = 0;
                foreach (var item in AllActive)
                {
                    //pull from def later
                    willTotal += 1;
                }

                return willTotal;
            }
        }


        public bool IsOverWillLimits => WillRequiredForUndead > this.undeadLimit;

        public IThingHolder ParentHolder => this.pawn.ParentHolder;

        public virtual IntVec3 LeaderPosition => this.pawn.Position;

        public List<Pawn> SquadMembersPawns => activeCreature.ToList();

        public Pawn SquadLeader => this.pawn;

        bool ISquadLeader.InFormation => this.InFormation;


        public SquadHostility SquadHostilityResponse = SquadHostility.Aggressive;
        public SquadHostility HostilityResponse => SquadHostilityResponse;

        public List<ISquadMember> _SquadMembers = new List<ISquadMember>();
        public List<ISquadMember> SquadMembers => _SquadMembers;

        bool ISquadLeader.ShowExtraOrders { get => this.ShowExtraOrders; set => this.ShowExtraOrders = value; }

        Lord ISquadLeader.SquadLord
        {
            get => this.SquadLord;
            set =>this.SquadLord  = value;
        }


        #endregion

        // Constructor to initialize ThingOwner
        public Hediff_UndeadMaster()
        {
            // Initialize ThingOwner with this as the owner
            storedCreature = new ThingOwner<Pawn>(this, false, LookMode.Deep);
            activeCreature = new HashSet<Pawn>();

          
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
            if (WillRequiredForUndead > this.undeadLimit)
            {
                //pick pawns until under limit, turn them feral

                Pawn pawn = this.AllActive.RandomElement();

                if (WillRequiredForUndead > this.undeadLimit)
                {
             
                }
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

        #region Formation Management
        public void SetFormation(FormationUtils.FormationType formationType)
        {
            _FormationType = formationType;
        }

        public void SetFollowDistance(float distance)
        {
            _FollowDistance = distance;
        }

        public void SetInFormation(bool inFormation)
        {
            InFormation = inFormation;
        }

        public void ToggleInFormation()
        {
            InFormation = !InFormation;
        }
        public void SetHositilityResponse(SquadHostility squadHostilityResponse)
        {
            SquadHostilityResponse = squadHostilityResponse;
        }

        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin)
        {
            if (activeCreature.Contains(pawn))
            {
                return FormationUtils.GetFormationPosition(
                    FormationType,
                    Origin.ToVector3(),
                    this.pawn.Rotation,
                    AllActive.IndexOf(pawn),
                    AllActive.Count);
            }

            return IntVec3.Invalid;
        }

        public IntVec3 GetFormationPositionFor(Pawn pawn)
        {
            return GetFormationPositionFor(pawn, this.pawn.Position);
        }
        #endregion

        #region Summoning Management
        /// <summary>
        /// Checks if the pawn is currently in the stored undead list.
        /// </summary>
        public bool IsCreatureStored(Pawn pawn)
        {
            return storedCreature.Contains(pawn);
        }

        /// <summary>
        /// Checks if a creature is currently active.
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>True if the pawn is in the active list</returns>
        public bool IsPartOfSquad(Pawn pawn)
        {
            return activeCreature.Contains(pawn);
        }

        /// <summary>
        /// Adds a given pawn to the stored creatures list, removes it from the active list if present,
        /// removes it from the map and stores it in the world.
        /// </summary>
        /// <param name="pawn">The pawn to store</param>
        public void StoreCreature(Pawn pawn)
        {
            if (activeCreature.Contains(pawn))
            {
                activeCreature.Remove(pawn);
            }


            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }


            pawn.relations.ClearAllRelations();
            // Must despawn before adding to ThingOwner
            if (pawn.Spawned)
            {
                pawn.DeSpawn(DestroyMode.Vanish);
            }

            if (!storedCreature.Contains(pawn))
            {
                storedCreature.TryAdd(pawn, false);
            }


            Log.Message($"Successfully stored creature {pawn.Label}");
        }

        public void SetupCreature(Pawn pawn)
        {
            Hediff_Undead undeadHediff = (Hediff_Undead)pawn.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_Undead);
            undeadHediff.SetSquadLeader(this.pawn);

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
            if (IsPartOfSquad(pawn))
            {
                Log.Message($"Tried to Summon an already active creature");
                return false;
            }

            // Check if stored
            if (!IsCreatureStored(pawn))
            {
                Log.Message($"Tried to Summon a creature that is not stored.");
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


        public bool AddToSquad(Pawn pawn)
        {
            if (!activeCreature.Contains(pawn))
            {
                activeCreature.Add(pawn);

                if (pawn.IsPartOfSquad(out ISquadMember squadMember))
                {
                    _SquadMembers.Add(squadMember);
                }

                //if (SquadLord != null)
                //{
                //    if (SquadLord.CanAddPawn(pawn))
                //    {
                //        SquadLord.AddPawn(pawn);
                //    }
                //}
                return true;
            }
            return false;
        }
        /// <summary>
        /// Completely removes a creature from both active and stored lists, and destroys it.
        /// </summary>
        /// <param name="pawn">The pawn to delete</param>
        public bool RemoveFromSquad(Pawn pawn, bool alsoDestroy = false)
        {
            if (activeCreature.Contains(pawn))
            {
                activeCreature.Remove(pawn);

                if (pawn.IsPartOfSquad(out ISquadMember squadMember))
                {
                    if (_SquadMembers.Contains(squadMember))
                    {
                        _SquadMembers.Remove(squadMember);
                    }
                }

                if (storedCreature.Contains(pawn))
                {
                    storedCreature.Remove(pawn);
                }
                //if (SquadLord != null)
                //{
                //    if (SquadLord.ownedPawns.Any(x=> x == pawn))
                //    {
                //        SquadLord.RemovePawn(pawn);
                //    }
                //}

                if (alsoDestroy)
                {
                    if (!pawn.Destroyed)
                    {
                        pawn.Destroy();
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Summons a stored creature in formation.
        /// </summary>
        /// <param name="pawn">The pawn to summon</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SummonCreatureInFormation(Pawn pawn)
        {
            if (!IsCreatureStored(pawn))
            {
                Log.Message($"Tried to Summon a creature that is not stored.");
                return false;
            }

            IntVec3 position = FormationUtils.GetFormationPosition(
                FormationType,
                this.pawn.Position.ToVector3Shifted(),
                this.pawn.Rotation,
                storedCreature.InnerListForReading.IndexOf(pawn),
                storedCreature.Count);

            return SummonCreature(pawn, position);
        }
        /// <summary>
        /// Unsummons an active creature, moving it back to storage.
        /// </summary>
        /// <param name="pawn">The pawn to unsummon</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool UnsummonCreature(Pawn pawn)
        {
            if (!activeCreature.Contains(pawn))
            {
                Log.Message("Tried to unsummon a creature is not in the active list");
                return false;
            }

            // Remove from active list first
            activeCreature.Remove(pawn);
            // Add to storage
            if (!storedCreature.Contains(pawn))
            {
                if (pawn.Spawned)
                {
                    pawn.DeSpawn(DestroyMode.Vanish);
                }
                storedCreature.TryAddOrTransfer(pawn, false);
            }

            return true;
        }

        /// <summary>
        /// Unsummons all active creatures.
        /// </summary>
        /// <returns>True if all were unsummoned successfully</returns>
        public bool UnSummonAll()
        {
            bool success = true;
            foreach (var creature in AllActive.ToList())
            {
                if (!UnsummonCreature(creature))
                {
                    success = false;
                }
            }

            return success;
        }

        #endregion

        #region Behavior Control

        public void SetAllState(SquadMemberState squadMemberState)
        {
            foreach (var creature in AllActive)
            {
                if (creature.IsControlledSummon(out Hediff_Undead undead))
                {
                    undead.SetCurrentMemberState(squadMemberState);
                }
            }
        }
        #endregion

        #region UI and Persistence
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Gizmo_FormationControl(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();


            Scribe_Deep.Look<ThingOwner<Pawn>>(ref storedCreature, "storedCursedSpirits", new object[] { this });

            Scribe_Collections.Look(ref activeCreature, "activeCursedSpirits", LookMode.Reference);

            Scribe_Values.Look(ref _FormationType, "formationType", FormationUtils.FormationType.Column);
            Scribe_Values.Look(ref _FollowDistance, "followDistance", 5f);
            Scribe_Values.Look(ref InFormation, "inFormation", true);
            Scribe_Values.Look(ref SquadHostilityResponse, "SquadHostilityResponse");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Initialize collections if they're null
                if (storedCreature == null)
                    storedCreature = new ThingOwner<Pawn>(this, false, LookMode.Deep);

                if (activeCreature == null)
                    activeCreature = new HashSet<Pawn>();

                // Clean up null references

                var wew = activeCreature.ToList();
                wew.RemoveAll(x => x == null);
                activeCreature =  new HashSet<Pawn>(wew);

                // Rebuild references between masters and summons
                // This ensures all active summons have proper hediffs
                foreach (var activePawn in activeCreature)
                {
                    Log.Message($"REINIT PAWN {activePawn.Label}");

                    if (activePawn.health != null)
                    {
                        Hediff_Undead undeadHediff = activePawn.health.hediffSet.GetFirstHediffOfDef(MagicAndMythDefOf.DeathKnight_Undead) as Hediff_Undead;
                        if (undeadHediff != null)
                        {
                            undeadHediff.SetSquadLeader(this.pawn);
                        }
                    }
                }
            }
        }



        #endregion
    }
}
