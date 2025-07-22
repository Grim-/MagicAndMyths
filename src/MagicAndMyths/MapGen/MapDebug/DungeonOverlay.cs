using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public abstract class DungeonOverlay
    {
        protected MapComp_DungeonGenDebugger debugger;
        protected Map map;
        protected Dungeon dungeon => debugger.Dungeon;

        public abstract string Label { get; }
        public abstract string Symbol { get; }
        public abstract Color Color { get; }
        public bool IsEnabled { get; set; }

        public DungeonOverlay(MapComp_DungeonGenDebugger debugger)
        {
            this.debugger = debugger;
            this.map = debugger.map;
        }

        public abstract IEnumerable<IntVec3> GetCells();

        public virtual void Draw()
        {
            if (!IsEnabled || dungeon == null)
            {
                return;
            }
               

            foreach (var cell in GetCells())
            {
                if (!cell.InBounds(map)) continue;

                Vector2 screenPos = GetCellScreenPosition(cell);
                if (!IsPositionVisible(screenPos)) continue;

                DrawSymbolAtPosition(screenPos, Symbol, Color);
            }
        }

        protected Vector2 GetCellScreenPosition(IntVec3 cell)
        {
            Vector3 worldPos = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Vector2 screenPos = Find.Camera.WorldToScreenPoint(worldPos);
            screenPos.y = Screen.height - screenPos.y;
            return screenPos;
        }

        protected bool IsPositionVisible(Vector2 screenPos)
        {
            return screenPos.x >= 0 && screenPos.x <= Screen.width &&
                   screenPos.y >= 0 && screenPos.y <= Screen.height;
        }

        protected void DrawSymbolAtPosition(Vector2 screenPos, string symbol, Color color)
        {
            Vector2 labelSize = Text.CalcSize(symbol);
            Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f, screenPos.y - labelSize.y / 2f, labelSize.x, labelSize.y);

            GUI.color = color;
            Widgets.Label(labelRect, symbol);
            GUI.color = Color.white;
        }
    }
}