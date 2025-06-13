using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace MagicAndMyths
{
    public class MapComp_DungeonGenDebugger : MapComponent
    {
        protected Dungeon Dungeon;
        private bool showDebugInfo = false;
        private bool showProtectionOverlay = false;
        private Vector2 scrollPos = Vector2.zero;

        public MapComp_DungeonGenDebugger(Map map) : base(map)
        {
        }

        public void SetDungeon(Dungeon dungeon)
        {
            this.Dungeon = dungeon;
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            if (!(map.Parent is DungeonMapParent dungeonParent))
                return;

            if (Dungeon == null && dungeonParent != null)
            {
                Dungeon = dungeonParent.GeneratedDungeon;
            }

            DrawDebugToggle();

            if (showDebugInfo && Dungeon != null)
            {
                DrawDebugPanel();
                DrawMapOverlays();
            }
        }

        private void DrawDebugToggle()
        {
            Rect toggleRect = new Rect(10f, 10f, 150f, 30f);
            if (Widgets.ButtonText(toggleRect, showDebugInfo ? "Hide Debug" : "Show Debug"))
            {
                showDebugInfo = !showDebugInfo;
            }
        }

        private void DrawDebugPanel()
        {
            Rect panelRect = new Rect(10f, 50f, 400f, 500f);
            Widgets.DrawWindowBackground(panelRect);

            Rect viewRect = new Rect(0f, 0f, 380f, CalculateContentHeight());
            Rect scrollRect = new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, panelRect.height - 20f);

            Widgets.BeginScrollView(scrollRect, ref scrollPos, viewRect);

            float curY = DrawHeader(viewRect.width);
            curY = DrawAutomataButton(viewRect.width, curY);
            curY = DrawOverlayToggles(viewRect.width, curY);
            curY = DrawSummaryInfo(viewRect.width, curY);
            curY = DrawCriticalPathInfo(viewRect.width, curY);
            curY = DrawSideRoomsInfo(viewRect.width, curY);
            DrawConnectionsInfo(viewRect.width, curY);

            Widgets.EndScrollView();
        }

        private float DrawOverlayToggles(float width, float startY)
        {
            Widgets.Label(new Rect(0f, startY, width, 25f), "Overlays:");
            startY += 25f;

            Rect protectionToggleRect = new Rect(10f, startY, width - 10f, 25f);
            bool newProtectionOverlay = showProtectionOverlay;
            Widgets.CheckboxLabeled(protectionToggleRect, "Show Protection Grid", ref newProtectionOverlay);
            if (newProtectionOverlay != showProtectionOverlay)
            {
                showProtectionOverlay = newProtectionOverlay;
            }
            startY += 30f;

            return startY + 10f;
        }

        private float DrawAutomataButton(float width, float startY)
        {
            Rect buttonRect = new Rect(0f, startY, width, 30f);
            if (Widgets.ButtonText(buttonRect, "Apply Cellular Automata"))
            {
                ShowCellularAutomataMenu();
            }
            return startY + 40f;
        }

        private void ShowCellularAutomataMenu()
        {
            var automataOptions = new List<FloatMenuOption>();

            var allAutomataDefs = DefDatabase<CelluarAutomataDef>.AllDefsListForReading;

            foreach (var automataDef in allAutomataDefs)
            {
                automataOptions.Add(new FloatMenuOption(automataDef.defName, () => ApplyCellularAutomata(automataDef)));
            }

            if (automataOptions.Count == 0)
            {
                automataOptions.Add(new FloatMenuOption("No cellular automata defs found", null));
            }

            Find.WindowStack.Add(new FloatMenu(automataOptions));
        }

        private void ApplyCellularAutomata(CelluarAutomataDef automataDef)
        {
            if (Dungeon == null)
            {
                Messages.Message("No dungeon found to apply automata to", MessageTypeDefOf.RejectInput);
                return;
            }

            var automataList = new List<CelluarAutomataSteps>
            {
                new CelluarAutomataSteps()
                {
                    automataDef = automataDef,
                    iterations = 5
                }
            };

            CellularAutomataManager.ApplyRules(map, Dungeon, automataList, 5);
            Messages.Message($"Applied {automataDef.defName} to dungeon", MessageTypeDefOf.TaskCompletion);
        }

        private float DrawHeader(float width)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, width, 30f), "Dungeon Debug Info");
            Text.Font = GameFont.Small;
            return 35f;
        }

        private float DrawSummaryInfo(float width, float startY)
        {
            var allRooms = Dungeon.GetAllRooms().ToList();
            var criticalPathRooms = allRooms.Where(r => r.IsOnCriticalPath).ToList();
            var sidePathRooms = allRooms.Where(r => !r.IsOnCriticalPath).ToList();

            Widgets.Label(new Rect(0f, startY, width, 25f), $"Total Rooms: {allRooms.Count}");
            startY += 25f;
            Widgets.Label(new Rect(0f, startY, width, 25f), $"Critical Path Rooms: {criticalPathRooms.Count}");
            startY += 25f;
            Widgets.Label(new Rect(0f, startY, width, 25f), $"Side Path Rooms: {sidePathRooms.Count}");
            return startY + 35f;
        }

        private float DrawCriticalPathInfo(float width, float startY)
        {
            Widgets.Label(new Rect(0f, startY, width, 25f), "Critical Path:");
            startY += 25f;

            var criticalPathRooms = Dungeon.GetAllRooms()
                .Where(r => r.IsOnCriticalPath)
                .OrderBy(r => r.CriticalPathIndex)
                .ToList();

            foreach (var room in criticalPathRooms)
            {
                string roomInfo = $"  [{room.CriticalPathIndex}] {room.def?.defName ?? "Unassigned"} at {room.Center}";
                Widgets.Label(new Rect(10f, startY, width - 10f, 20f), roomInfo);
                startY += 20f;
            }

            return startY + 10f;
        }

        private float DrawSideRoomsInfo(float width, float startY)
        {
            Widgets.Label(new Rect(0f, startY, width, 25f), "Side Rooms:");
            startY += 25f;

            var sidePathRooms = Dungeon.GetAllRooms().Where(r => !r.IsOnCriticalPath).ToList();

            foreach (var room in sidePathRooms)
            {
                string roomInfo = $"  {room.def?.defName ?? "Unassigned"} at {room.Center}";
                roomInfo += GetRoomTags(room);
                Widgets.Label(new Rect(10f, startY, width - 10f, 20f), roomInfo);
                startY += 20f;
            }

            return startY + 10f;
        }

        private string GetRoomTags(DungeonRoom room)
        {
            string tags = "";
            if (room.HasTag("side_path")) tags += " [SIDE_PATH]";
            if (room.HasTag("hidden")) tags += " [HIDDEN]";
            return tags;
        }

        private void DrawConnectionsInfo(float width, float startY)
        {
            Widgets.Label(new Rect(0f, startY, width, 25f), "Room Connections:");
            startY += 25f;

            var allRooms = Dungeon.GetAllRooms().ToList();
            foreach (var room in allRooms)
            {
                if (room.connectedRooms?.Count > 0)
                {
                    string connectionInfo = $"  {room.def?.defName ?? "Room"} -> {room.connectedRooms.Count} connections";
                    Widgets.Label(new Rect(10f, startY, width - 10f, 20f), connectionInfo);
                    startY += 20f;
                }
            }
        }

        private void DrawMapOverlays()
        {
            var allRooms = Dungeon.GetAllRooms().ToList();
            DrawCriticalPathLine(allRooms);
            DrawRoomLabels(allRooms);

            if (showProtectionOverlay)
            {
                DrawProtectionOverlay();
            }
        }

        private void DrawProtectionOverlay()
        {
            if (Dungeon?.GridManager.ProtectionGrid == null) return;

            foreach (IntVec3 cell in map.AllCells)
            {
                if (!cell.InBounds(map)) continue;

                bool isProtected = Dungeon.GridManager.ProtectionGrid[cell];
                if (!isProtected) continue;

                Vector3 worldPos = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                Vector2 screenPos = Find.Camera.WorldToScreenPoint(worldPos);
                screenPos.y = Screen.height - screenPos.y;

                if (!IsPositionVisible(screenPos)) continue;

                Vector2 labelSize = Text.CalcSize("P");
                Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f, screenPos.y - labelSize.y / 2f, labelSize.x, labelSize.y);

                GUI.color = Color.blue;
                Widgets.Label(labelRect, "P");
                GUI.color = Color.white;
            }
        }

        private void DrawRoomLabels(System.Collections.Generic.List<DungeonRoom> allRooms)
        {
            foreach (var room in allRooms)
            {
                Vector2 screenPos = GetRoomScreenPosition(room);
                if (!IsPositionVisible(screenPos)) continue;

                string label = GetRoomLabel(room);
                Color labelColor = GetRoomLabelColor(room);
                DrawLabelWithShadow(screenPos, label, labelColor);
            }
        }

        private Vector2 GetRoomScreenPosition(DungeonRoom room)
        {
            Vector3 worldPos = room.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Vector2 screenPos = Find.Camera.WorldToScreenPoint(worldPos);
            screenPos.y = Screen.height - screenPos.y;
            return screenPos;
        }

        private bool IsPositionVisible(Vector2 screenPos)
        {
            return screenPos.x >= 0 && screenPos.x <= Screen.width &&
                   screenPos.y >= 0 && screenPos.y <= Screen.height;
        }

        private string GetRoomLabel(DungeonRoom room)
        {
            string label = room.def?.defName ?? "Unassigned";
            if (room.IsOnCriticalPath)
            {
                label = $"{label} [{room.CriticalPathIndex}]";
            }

            if (GetRoomTags(room).Count() > 0)
            {
                label += $"\r\ntags : {GetRoomTags(room)}";
            }
            return label;
        }

        private Color GetRoomLabelColor(DungeonRoom room)
        {
            if (room.IsOnCriticalPath)
                return Color.red;
            if (room.HasTag("hidden"))
                return Color.green;
            if (room.HasTag("side_path"))
                return Color.yellow;
            return Color.white;
        }

        private void DrawLabelWithShadow(Vector2 screenPos, string label, Color labelColor)
        {
            Vector2 labelSize = Text.CalcSize(label);
            Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f, screenPos.y - labelSize.y / 2f, labelSize.x, labelSize.y);

            GUI.color = Color.black;
            Widgets.Label(labelRect.ExpandedBy(1f, 1f), label);

            GUI.color = labelColor;
            Widgets.Label(labelRect, label);

            GUI.color = Color.white;
        }

        private void DrawCriticalPathLine(List<DungeonRoom> allRooms)
        {
            var criticalPathRooms = allRooms.Where(r => r.IsOnCriticalPath)
                                           .OrderBy(r => r.CriticalPathIndex)
                                           .ToList();

            if (criticalPathRooms.Count < 2) return;

            for (int i = 0; i < criticalPathRooms.Count - 1; i++)
            {
                Vector3 pointA = criticalPathRooms[i].Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                Vector3 pointB = criticalPathRooms[i + 1].Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);

                GenDraw.DrawLineBetween(pointA, pointB);
            }
        }

        private float CalculateContentHeight()
        {
            if (Dungeon == null) return 100f;

            var allRooms = Dungeon.GetAllRooms().ToList();
            var criticalPathRooms = allRooms.Where(r => r.IsOnCriticalPath).ToList();
            var sidePathRooms = allRooms.Where(r => !r.IsOnCriticalPath).ToList();

            return 240f + (criticalPathRooms.Count * 20f) + (sidePathRooms.Count * 20f) + (allRooms.Count * 20f) + 100f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showDebugInfo, "showDebugInfo", false);
            Scribe_Values.Look(ref showProtectionOverlay, "showProtectionOverlay", false);
        }
    }
}