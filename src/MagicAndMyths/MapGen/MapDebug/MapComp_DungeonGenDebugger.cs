using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace MagicAndMyths
{
    public class MapComp_DungeonGenDebugger : MapComponent
    {
        private const float TOGGLE_BUTTON_WIDTH = 150f;
        private const float TOGGLE_BUTTON_HEIGHT = 30f;
        private const float PANEL_WIDTH = 400f;
        private const float PANEL_HEIGHT = 500f;
        private const float PANEL_PADDING = 10f;
        private const float SCROLL_PADDING = 20f;
        private const float LINE_HEIGHT = 25f;
        private const float ITEM_HEIGHT = 20f;
        private const float BUTTON_HEIGHT = 30f;
        private const float SECTION_SPACING = 10f;
        private const float INDENT = 10f;
        private const float LABEL_SHADOW_EXPAND = 1f;
        private const float BASE_HEIGHT = 300f;
        private const float EXTRA_HEIGHT = 100f;

        private static readonly Color CRITICAL_PATH_COLOR = Color.red;
        private static readonly Color HIDDEN_ROOM_COLOR = Color.green;
        private static readonly Color SIDE_PATH_COLOR = Color.yellow;
        private static readonly Color DEFAULT_ROOM_COLOR = Color.white;
        private static readonly Color SHADOW_COLOR = Color.black;

        public Dungeon Dungeon { get; protected set; }
        private bool showDebugInfo = true;
        private Vector2 scrollPos = Vector2.zero;

        private List<DungeonOverlay> overlays;

        public MapComp_DungeonGenDebugger(Map map) : base(map)
        {
            InitializeOverlays();
        }

        private void InitializeOverlays()
        {
            overlays = new List<DungeonOverlay>
            {
                new ProtectionOverlay(this),
                new CorridorOverlay(this),
                new RoomOverlay(this),
                new BspOverlay(this),
                new BspTreeTraversalOverlay(this)
            };
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
            Rect toggleRect = new Rect(PANEL_PADDING, PANEL_PADDING, TOGGLE_BUTTON_WIDTH, TOGGLE_BUTTON_HEIGHT);
            if (Widgets.ButtonText(toggleRect, showDebugInfo ? "Hide Debug" : "Show Debug"))
            {
                showDebugInfo = !showDebugInfo;
            }
        }

        private void DrawDebugPanel()
        {
            Rect panelRect = new Rect(PANEL_PADDING, PANEL_PADDING + TOGGLE_BUTTON_HEIGHT + PANEL_PADDING, PANEL_WIDTH, PANEL_HEIGHT);
            Widgets.DrawWindowBackground(panelRect);

            float contentWidth = PANEL_WIDTH - SCROLL_PADDING;
            Rect viewRect = new Rect(0f, 0f, contentWidth, CalculateContentHeight());
            Rect scrollRect = new Rect(panelRect.x + PANEL_PADDING, panelRect.y + PANEL_PADDING, panelRect.width - SCROLL_PADDING, panelRect.height - SCROLL_PADDING);

            Widgets.BeginScrollView(scrollRect, ref scrollPos, viewRect);

            float curY = DrawHeader(contentWidth);
            curY = DrawRegenButton(contentWidth, curY);
            curY = DrawStepGenButton(contentWidth, curY);
            curY = DrawAutomataButton(contentWidth, curY);
            curY = DrawOverlayToggles(contentWidth, curY);
            curY = DrawSummaryInfo(contentWidth, curY);
            Widgets.EndScrollView();
        }

        private float DrawHeader(float width)
        {
            Text.Font = GameFont.Medium;
            DrawLabel(0f, 0f, width, BUTTON_HEIGHT, "Dungeon Debug Info");
            Text.Font = GameFont.Small;
            return BUTTON_HEIGHT + SECTION_SPACING;
        }

        private float DrawAutomataButton(float width, float startY)
        {
            Rect buttonRect = new Rect(0f, startY, width, BUTTON_HEIGHT);
            if (Widgets.ButtonText(buttonRect, "Apply Cellular Automata"))
            {
                ShowCellularAutomataMenu();
            }
            return startY + BUTTON_HEIGHT + SECTION_SPACING;
        }
        private float DrawRegenButton(float width, float startY)
        {
            if (!(map.Parent is DungeonMapParent dungeonParent))
                return startY;

            Rect buttonRect = new Rect(0f, startY, width, BUTTON_HEIGHT);
            if (Widgets.ButtonText(buttonRect, "Regen Map"))
            {
                dungeonParent.DungeonGen.Regenerate();
            }
            return startY + BUTTON_HEIGHT + SECTION_SPACING;
        }

        private float DrawStepGenButton(float width, float startY)
        {
            if (!(map.Parent is DungeonMapParent dungeonParent))
                return startY;

            Rect buttonRect = new Rect(0f, startY, width, BUTTON_HEIGHT);
            if (Widgets.ButtonText(buttonRect, "Step Generation"))
            {
                dungeonParent.DungeonGen.StepGeneration();
            }
            return startY + BUTTON_HEIGHT + SECTION_SPACING;
        }
        private float DrawOverlayToggles(float width, float startY)
        {
            startY = DrawSectionHeader(width, startY, "Overlays:");

            foreach (var overlay in overlays)
            {
                startY = DrawCheckbox(width, startY, overlay.Label, overlay.IsEnabled, value => overlay.IsEnabled = value);
            }

            return startY + SECTION_SPACING;
        }

        private float DrawSummaryInfo(float width, float startY)
        {
            var allRooms = Dungeon.GetAllRooms().ToList();
            var criticalPathRooms = allRooms.Where(r => r.IsOnCriticalPath).ToList();
            var sidePathRooms = allRooms.Where(r => !r.IsOnCriticalPath).ToList();

            var summaryItems = new[]
            {
                $"Total Rooms: {allRooms.Count}",
                $"Critical Path Rooms: {criticalPathRooms.Count}",
                $"Side Path Rooms: {sidePathRooms.Count}"
            };

            foreach (var item in summaryItems)
            {
                startY = DrawLabel(0f, startY, width, LINE_HEIGHT, item);
            }

            return startY + SECTION_SPACING;
        }

        private float DrawSectionHeader(float width, float startY, string text)
        {
            return DrawLabel(0f, startY, width, LINE_HEIGHT, text);
        }

        private float DrawLabel(float x, float y, float width, float height, string text)
        {
            Widgets.Label(new Rect(x, y, width, height), text);
            return y + height;
        }

        private float DrawIndentedLabel(float width, float startY, string text)
        {
            return DrawLabel(INDENT, startY, width - INDENT, ITEM_HEIGHT, text);
        }

        private float DrawCheckbox(float width, float startY, string label, bool currentValue, System.Action<bool> setter)
        {
            Rect checkboxRect = new Rect(INDENT, startY, width - INDENT, LINE_HEIGHT);
            bool newValue = currentValue;
            Widgets.CheckboxLabeled(checkboxRect, label, ref newValue);
            if (newValue != currentValue)
            {
                setter(newValue);
            }
            return startY + BUTTON_HEIGHT;
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

        private string GetRoomTags(DungeonRoom room)
        {
            var tags = new List<string>();
            if (room.HasTag("side_path")) tags.Add("[SIDE_PATH]");
            if (room.HasTag("hidden")) tags.Add("[HIDDEN]");
            return tags.Any() ? " " + string.Join(" ", tags) : "";
        }

        private void DrawMapOverlays()
        {
            var allRooms = Dungeon.GetAllRooms().ToList();
            DrawCriticalPathLine(allRooms);
            DrawRoomLabels(allRooms);

            foreach (var overlay in overlays)
            {
                overlay.Draw();
            }
        }

        private void DrawRoomLabels(List<DungeonRoom> allRooms)
        {
            foreach (var room in allRooms)
            {
                Vector2 screenPos = GetCellScreenPosition(room.Center);
                if (!IsPositionVisible(screenPos)) continue;

                string label = GetRoomLabel(room);
                Color labelColor = GetRoomLabelColor(room);
                DrawLabelWithShadow(screenPos, label, labelColor);
            }
        }

        private Vector2 GetCellScreenPosition(IntVec3 cell)
        {
            Vector3 worldPos = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
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

            string tags = GetRoomTags(room);
            if (!string.IsNullOrEmpty(tags))
            {
                label += $"\r\ntags : {tags}";
            }
            return label;
        }

        private Color GetRoomLabelColor(DungeonRoom room)
        {
            if (room.IsOnCriticalPath) return CRITICAL_PATH_COLOR;
            if (room.HasTag("hidden")) return HIDDEN_ROOM_COLOR;
            if (room.HasTag("side_path")) return SIDE_PATH_COLOR;
            return DEFAULT_ROOM_COLOR;
        }

        private void DrawLabelWithShadow(Vector2 screenPos, string label, Color labelColor)
        {
            Vector2 labelSize = Text.CalcSize(label);
            Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f, screenPos.y - labelSize.y / 2f, labelSize.x, labelSize.y);

            GUI.color = SHADOW_COLOR;
            Widgets.Label(labelRect.ExpandedBy(LABEL_SHADOW_EXPAND, LABEL_SHADOW_EXPAND), label);

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

            return BASE_HEIGHT +
                   EXTRA_HEIGHT;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showDebugInfo, "showDebugInfo", false);

            if (overlays != null)
            {
                for (int i = 0; i < overlays.Count; i++)
                {
                    bool isEnabled = overlays[i].IsEnabled;
                    Scribe_Values.Look(ref isEnabled, $"overlay_{overlays[i].GetType().Name}_enabled", false);
                    overlays[i].IsEnabled = isEnabled;
                }
            }
        }
    }
}