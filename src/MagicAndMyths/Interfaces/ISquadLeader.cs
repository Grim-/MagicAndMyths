using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public interface ISquadLeader
    {
        Pawn SquadLeader { get; }

        Lord SquadLord { get; set; }

        IntVec3 LeaderPosition { get; }
        List<Pawn> SquadMembersPawns { get; }
        List<ISquadMember> SquadMembers { get; }
        FormationUtils.FormationType FormationType { get; }
        float FollowDistance { get; }
        bool InFormation { get; }

        bool ShowExtraOrders { get; set; }
        SquadHostility HostilityResponse { get; }
        void SetHositilityResponse(SquadHostility squadHostilityResponse);

        void SetFormation(FormationUtils.FormationType formationType);
        void SetFollowDistance(float distance);
        void SetInFormation(bool inFormation);

        bool AddToSquad(Pawn pawn);
        bool RemoveFromSquad(Pawn pawn, bool alsoDestroy = false);
        void SetAllState(SquadMemberState squadMemberState);
        void ToggleInFormation();
        bool IsPartOfSquad(Pawn pawn);


        IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin);
        IntVec3 GetFormationPositionFor(Pawn pawn);
    }
}
