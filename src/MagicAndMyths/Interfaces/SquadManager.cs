using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    //public interface ISquadLeader
    //{
    //    Pawn SquadLeaderPawn { get; }

    //    Lord SquadLord { get; set; }

    //    IntVec3 LeaderPosition { get; }
    //    List<Pawn> SquadMembersPawns { get; }
    //    List<ISquadMember> SquadMembers { get; }
    //    FormationUtils.FormationType FormationType { get; }


    //    SquadMemberState SquadState { get; }

    //    float AggresionDistance { get; }
    //    float FollowDistance { get; }
    //    bool InFormation { get; }

    //    bool ShowExtraOrders { get; set; }
    //    SquadHostility HostilityResponse { get; }
    //    void SetHositilityResponse(SquadHostility squadHostilityResponse);

    //    void SetFormation(FormationUtils.FormationType formationType);
    //    void SetFollowDistance(float distance);
    //    void SetInFormation(bool inFormation);


    //    void ExecuteSquadOrder(SquadOrderDef orderDef, LocalTargetInfo target);

    //    bool AddToSquad(Pawn pawn);
    //    bool RemoveFromSquad(Pawn pawn, bool kill = true, bool alsoDestroy = false);
    //    void SetAllState(SquadMemberState squadMemberState);
    //    void ToggleInFormation();
    //    bool IsPartOfSquad(Pawn pawn);


    //    IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin);
    //    IntVec3 GetFormationPositionFor(Pawn pawn);
    //}
    public class SquadManager : GameComponent
    {
        private Dictionary<Pawn, List<Squad>> squadCache = new Dictionary<Pawn, List<Squad>>();

        public SquadManager(Game game) : base()
        { 
        
        }

        public void RegisterSquad(Pawn leader, Squad squad)
        {
            if (!squadCache.ContainsKey(leader))
            {
                squadCache[leader] = new List<Squad>();
            }
            squadCache[leader].Add(squad);
        }

        public void RemoveSquad(Pawn leader, Squad squad)
        {
            if (squadCache.ContainsKey(leader))
            {
                squadCache[leader].Remove(squad);
                if (squadCache[leader].Count == 0)
                {
                    squadCache.Remove(leader);
                }
            }
        }

        public List<Squad> GetSquadsForLeader(Pawn leader)
        {
            return squadCache.TryGetValue(leader, out var squads) ? squads : new List<Squad>();
        }


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref squadCache, "squadCache", LookMode.Reference, LookMode.Deep);
        }

    }
}
