using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class LordJob_DungeonEncounter : LordJob
    {
        private Faction faction;
        private CellRect roomRect;
        private IntVec3 roomCenter;
        private bool allowLeaveRoom = false;

        public LordJob_DungeonEncounter()
        {
        }

        public LordJob_DungeonEncounter(Faction faction, CellRect roomRect)
        {
            this.faction = faction;
            this.roomRect = roomRect;
            this.roomCenter = roomRect.CenterCell;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            // Main behavior - defend the room but stay inside it
            LordToil_DungeonDefend lordToil_DungeonDefend = new LordToil_DungeonDefend(roomRect, roomCenter);
            stateGraph.StartingToil = lordToil_DungeonDefend;

            // Optional - if flagged to allow leaving room
            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony(true, false);
            lordToil_AssaultColony.useAvoidGrid = true;
            stateGraph.AddToil(lordToil_AssaultColony);

            // Transition when allowLeaveRoom is set to true (via your flag system)
            Transition leaveRoomTransition = new Transition(lordToil_DungeonDefend, lordToil_AssaultColony, false, true);
            leaveRoomTransition.AddTrigger(new Trigger_Custom(delegate { return allowLeaveRoom; }));
            leaveRoomTransition.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(leaveRoomTransition, false);

            return stateGraph;
        }

        // Method to be called by your flag system
        public void SetAllowLeaveRoom(bool allow)
        {
            allowLeaveRoom = allow;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Faction>(ref faction, "faction", false);
            Scribe_Values.Look<CellRect>(ref roomRect, "roomRect");
            Scribe_Values.Look<IntVec3>(ref roomCenter, "roomCenter", default(IntVec3), false);
            Scribe_Values.Look<bool>(ref allowLeaveRoom, "allowLeaveRoom", false, false);
        }
    }
}
