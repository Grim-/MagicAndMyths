﻿using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class MapComp_DungeonEnemies : MapComponent
    {
        private Dictionary<int, Lord> dungeonLords = new Dictionary<int, Lord>();

        public MapComp_DungeonEnemies(Map map) : base(map)
        {
        }

        public void AddLord(int roomID, Lord lord)
        {
            if (dungeonLords.ContainsKey(roomID))
            {
                dungeonLords[roomID] = lord;
            }
            else
            {
                dungeonLords.Add(roomID, lord);
            }
        }

        public void SetRoomPawnsCanLeave(int roomID, bool canLeave)
        {
            if (dungeonLords.TryGetValue(roomID, out Lord lord))
            {
                LordJob_DungeonEncounter lordJob = lord.LordJob as LordJob_DungeonEncounter;
                if (lordJob != null)
                {
                    lordJob.SetAllowLeaveRoom(canLeave);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref dungeonLords, "dungeonLords", LookMode.Value, LookMode.Reference);
        }
    }
}

