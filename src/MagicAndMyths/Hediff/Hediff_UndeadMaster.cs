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


        public int WillStat => Mathf.CeilToInt(this.pawn.GetStatValue(MagicAndMythDefOf.MagicAndMyths_Will, true, 1200));

        public float WillCapacityAsPercent => (float)WillRequiredForUndead / (float)WillStat;

        // Only store the unspawned pawns in a ThingOwner, if it fucking worked
        private ThingOwner storedCreature;

        // Keep active creatures as a simple list since they're spawned on the map
       // private HashSet<Pawn> activeCreature = new HashSet<Pawn>();

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

        public List<Pawn> AllActive => _ActiveSquads.SelectMany(x=> x.Value.Members.ToList()).ToList();

        public List<Pawn> AllStored => storedCreature.OfType<Pawn>().ToList();

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


        public bool IsOverWillLimits => WillRequiredForUndead > this.WillStat;

        public IThingHolder ParentHolder => this.pawn.ParentHolder;

        public virtual IntVec3 LeaderPosition => this.pawn.Position;

        public List<Pawn> SquadMembersPawns
        {
            get
            {
                List<Pawn> allPawns = new List<Pawn>();
                // Order by squad ID for consistent iteration
                foreach (var item in _ActiveSquads.OrderBy(x => x.Key))
                {
                    foreach (var member in item.Value.Members)
                    {
                        allPawns.Add(member);
                    }
                }
                return allPawns;
            }
        }


        public Pawn SquadLeaderPawn => this.pawn;

       // bool ISquadLeader.InFormation => this.InFormation;


        //public SquadHostility SquadHostilityResponse = SquadHostility.Aggressive;
        //public SquadHostility HostilityResponse => SquadHostilityResponse;

        public List<ISquadMember> _SquadMembers = new List<ISquadMember>();
        public List<ISquadMember> SquadMembers => _SquadMembers;

        bool ISquadLeader.ShowExtraOrders { get => this.ShowExtraOrders; set => this.ShowExtraOrders = value; }

        Lord ISquadLeader.SquadLord
        {
            get => this.SquadLord;
            set =>this.SquadLord  = value;
        }

        public float AggresionDistance => FollowDistance;

        public SquadMemberState currentSquadState = SquadMemberState.CalledToArms;
        public SquadMemberState SquadState => currentSquadState;



        private Dictionary<int, Squad> _ActiveSquads = new Dictionary<int, Squad>();
        public Dictionary<int, Squad> ActiveSquads => _ActiveSquads;


        #endregion

        // Constructor to initialize ThingOwner
        public Hediff_UndeadMaster()
        {
            // Initialize ThingOwner with this as the owner
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

                Pawn pawn = this.AllActive.RandomElement();

                if (WillRequiredForUndead > this.WillStat)
                {
             
                }
            }
        }

        public bool AddSquad(int squadID, List<Pawn> startingMembers)
        {
            if (!_ActiveSquads.ContainsKey(squadID))
            {
                _ActiveSquads.Add(squadID, CreateSquad(squadID));
                return true;
            }

            return false;
        }

        public Squad GetOrAddSquad(int squadID)
        {
            if (_ActiveSquads.ContainsKey(squadID))
            {
                return _ActiveSquads[squadID];
            }

            Squad newSquad = CreateSquad(squadID);
            _ActiveSquads.Add(squadID, newSquad);
            return newSquad;
        }
        public Squad GetFirstOrAddSquad()
        {
            if (_ActiveSquads == null || _ActiveSquads.Count == 0)
            {
                Squad newSquad = CreateSquad(1);
                _ActiveSquads.Add(1, newSquad);
                return newSquad;
            }
            return _ActiveSquads.Values.First();
        }

        public bool RemoveSquad(int squadID)
        {
            if (_ActiveSquads.ContainsKey(squadID))
            {
                _ActiveSquads.Remove(squadID);
                return true;
            }
            return false;
        }


        public Squad CreateSquad(int squadID)
        {
            return new Squad(squadID, this.pawn, FormationType, SquadHostility.Defensive);
        }


        public bool HasAnySquad()
        {
            return _ActiveSquads != null && _ActiveSquads.Count > 0;
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
            foreach (var item in _ActiveSquads)
            {
                item.Value.SetFormation(formationType);
            }

            _FormationType = formationType;
        }

        public void SetFollowDistance(float distance)
        {
            foreach (var item in _ActiveSquads)
            {
                item.Value.SetFollowDistance(distance);
            }
            _FollowDistance = distance;
        }

        public void SetInFormation(bool inFormation)
        {
            foreach (var item in _ActiveSquads)
            {
                item.Value.SetInFormation(inFormation);
            }
            InFormation = inFormation;
        }

        public void ToggleInFormation()
        {
            foreach (var item in _ActiveSquads)
            {
                item.Value.SetInFormation(!item.Value.InFormation);
            }
            InFormation = !InFormation;
        }
        public void SetHositilityResponse(SquadHostility squadHostilityResponse)
        {
           //SquadHostilityResponse = squadHostilityResponse;
        }

        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
        {
            foreach (var squad in _ActiveSquads.OrderBy(x => x.Key))
            {
                if (squad.Value.Members.Contains(pawn))
                {
                    return squad.Value.GetFormationPositionFor(pawn, Origin, OriginRotation);
                }
            }

            return IntVec3.Invalid;
        }

        public IntVec3 GetFormationPositionFor(Pawn pawn)
        {
            return GetFormationPositionFor(pawn, this.pawn.Position, this.pawn.Rotation);
        }
        #endregion

        #region Summoning Management

        /// <summary>
        /// Checks if a creature is currently active.
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>True if the pawn is in the active list</returns>
        public bool IsPartOfSquad(Pawn pawn)
        {
            return _ActiveSquads.Any(x=> x.Value.Members.Contains(pawn));
        }

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
            if (IsPartOfSquad(pawn))
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


        public bool IsPartOfAnySquad(Pawn pawn, out Squad squad)
        {
            squad = null;

            foreach (var item in _ActiveSquads)
            {
                if (item.Value.Members.Contains(pawn))
                {
                    squad = item.Value;
                    return true;
                }
            }

            return false;
        }

        public Squad GetSquadForPawn(Pawn pawn)
        {
            foreach (var item in _ActiveSquads)
            {
                if (item.Value.Members.Contains(pawn))
                {
                    return item.Value;
                }
            }

            return null;
        }
        public bool AddToSquad(Pawn pawn)
        {
            if (!IsPartOfAnySquad(pawn, out Squad squad))
            {
                Squad newSquad = GetFirstOrAddSquad();

                if (!newSquad.Members.Contains(pawn))
                {
                    newSquad.Members.Add(pawn);

                    if (pawn.IsPartOfSquad(out ISquadMember squadMember))
                    {
                        _SquadMembers.Add(squadMember);
                        squadMember.AssignedSquad = newSquad;
                        squadMember.SetSquadLeader(this.pawn);
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Completely removes a creature from both active and stored lists, and destroys it.
        /// </summary>
        /// <param name="pawn">The pawn to delete</param>
        public bool RemoveFromSquad(Pawn pawn, bool kill = true, bool alsoDestroy = false)
        {
            if (IsPartOfAnySquad(pawn, out Squad squad))
            {

                if (squad.Members.Contains(pawn))
                {
                    squad.RemoveMember(pawn);
                }

                if (pawn.IsPartOfSquad(out ISquadMember squadMember))
                {
                    if (_SquadMembers.Contains(squadMember))
                    {
                        _SquadMembers.Remove(squadMember);
                        squadMember.SetSquadLeader(null);
                        squadMember.AssignedSquad = null;
                    }
                }


                if (pawn.health.hediffSet.TryGetHediff<Hediff_Undead>(out Hediff_Undead undead))
                {
                    pawn.health.RemoveHediff(undead);
                }

                if (kill && pawn.Spawned && !pawn.health.Dead)
                {
                    pawn.Kill(null);
                }

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

            currentSquadState = squadMemberState;
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

        public void ExecuteSquadOrder(SquadOrderDef orderDef, LocalTargetInfo target)
        {
            foreach (var squadMember in SquadMembersPawns)
            {
                if (squadMember.IsPartOfSquad(out ISquadMember member))
                {
                    SquadOrderWorker squadOrderWorker = orderDef.CreateWorker(this, member);

                    if (squadOrderWorker.CanExecuteOrder(target))
                    {
                        squadOrderWorker.ExecuteOrder(target);
                    }
                }
            }
        }




        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look<ThingOwner>(ref storedCreature, "storedCursedSpirits", new object[] { this });
            Scribe_Collections.Look(ref _ActiveSquads, "activeSquads", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref _FormationType, "formationType", FormationUtils.FormationType.Column);
            Scribe_Values.Look(ref _FollowDistance, "followDistance", 5f);
            Scribe_Values.Look(ref InFormation, "inFormation", true);
        }

        #endregion
    }
}
