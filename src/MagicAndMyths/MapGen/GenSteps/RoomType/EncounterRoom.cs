using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class EncounterRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, CellRect RoomCellRect)
        {
            TerrainDef terrainDef = TerrainDefOf.Ice;
            DungeonUtil.SpawnTerrainForRoom(map, RoomCellRect, terrainDef);

            Generate(map, new IntRange(3, 8).RandomInRange, RoomCellRect, new List<PawnKindDef>
            {
                PawnKindDefOf.Pirate,
            }, Faction.OfAncientsHostile);
        }


        public void Generate(Map map, int numEnemies, CellRect roomRect, List<PawnKindDef> possibleEnemies, Faction faction)
        {
            List<Pawn> spawn = new List<Pawn>();


            for (int i = 0; i < numEnemies; i++)
            {
                PawnKindDef enemyKind = possibleEnemies.RandomElement();
                Pawn enemy = PawnGenerator.GeneratePawn(enemyKind, faction);
                GenSpawn.Spawn(enemy, roomRect.Cells.RandomElement(), map);
                spawn.Add(enemy);
            }


            LordJob_DefendBase lordJob = new LordJob_DefendBase(faction, roomRect.CenterCell);
            Lord enemyLord = LordMaker.MakeNewLord(faction, lordJob, map, spawn);
            map.GetComponent<MapComponent_DungeonEnemies>().AddLord(map.uniqueID, enemyLord);
        }
    }



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
