using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class LordToil_DungeonDefend : LordToil
    {
        private CellRect roomRect;
        private IntVec3 defendCenter;
        private float defendRadius;

        public LordToil_DungeonDefend(CellRect roomRect, IntVec3 defendCenter)
        {
            this.roomRect = roomRect;
            this.defendCenter = defendCenter;
            // Calculate defend radius based on room size, with some padding
            this.defendRadius = Mathf.Min(roomRect.Width, roomRect.Height) / 2f - 1f;
        }


        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, defendCenter, defendRadius);
                // Force pawns to stay within the room rect
                pawn.mindState.duty.locomotion = LocomotionUrgency.Walk;
                pawn.mindState.duty.wanderRadius = defendRadius;
                // pawn.mindState.duty.roomRect = roomRect; 
            }
        }

        public override void LordToilTick()
        {
            //foreach (Pawn pawn in lord.ownedPawns)
            //{
            //    if (!roomRect.Contains(pawn.Position) && pawn.Spawned)
            //    {
            //        pawn.mindState.duty.focus = defendCenter;
            //        pawn.mindState.duty.radius = 2f;
            //    }
            //}
        }
    }
}
