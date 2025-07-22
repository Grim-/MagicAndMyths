using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class RoomOverlay : DungeonOverlay
    {
        private static readonly Color CRITICAL_PATH_COLOR = Color.red;
        private static readonly Color HIDDEN_ROOM_COLOR = Color.green;
        private static readonly Color SIDE_PATH_COLOR = Color.yellow;
        private static readonly Color DEFAULT_ROOM_COLOR = Color.white;

        public override string Label => "Show Room Cells";
        public override string Symbol => "R";
        public override Color Color => DEFAULT_ROOM_COLOR;

        public RoomOverlay(MapComp_DungeonGenDebugger debugger) : base(debugger) { }

        public override IEnumerable<IntVec3> GetCells()
        {
            if (dungeon == null) return Enumerable.Empty<IntVec3>();

            var allCells = new List<IntVec3>();
            foreach (var room in dungeon.GetAllRooms())
            {
                allCells.AddRange(room.roomCells);
            }
            return allCells;
        }

        public override void Draw()
        {
            if (!IsEnabled || dungeon == null) return;

            foreach (var room in dungeon.GetAllRooms())
            {
                Color roomColor = GetRoomColor(room);
                foreach (var cell in room.roomCells)
                {
                    if (!cell.InBounds(map)) continue;

                    Vector2 screenPos = GetCellScreenPosition(cell);
                    if (!IsPositionVisible(screenPos)) continue;

                    DrawSymbolAtPosition(screenPos, Symbol, roomColor);
                }
            }
        }

        private Color GetRoomColor(DungeonRoom room)
        {
            if (room.IsOnCriticalPath) return CRITICAL_PATH_COLOR;
            if (room.HasTag("hidden")) return HIDDEN_ROOM_COLOR;
            if (room.HasTag("side_path")) return SIDE_PATH_COLOR;
            return DEFAULT_ROOM_COLOR;
        }
    }
}