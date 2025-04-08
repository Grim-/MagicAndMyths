using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class MapComp_ModifierManager : MapComponent
    {
        private List<MapModifier> activeModifiers = new List<MapModifier>();
        private static readonly Vector2 iconSize = new Vector2(32f, 32f);
        private static readonly float padding = 5f;
        private bool showModifiers = true;

        public MapComp_ModifierManager(Map map) : base(map)
        {
        }

        public void AddModifier(MapModifier modifier)
        {
            activeModifiers.Add(modifier);
        }

        public void RemoveModifier(MapModifier modifier)
        {
            activeModifiers.Remove(modifier);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            MapTick();
        }

        public void MapTick()
        {
            for (int i = activeModifiers.Count - 1; i >= 0; i--)
            {
                if (activeModifiers[i].ShouldRemove())
                {
                    activeModifiers.RemoveAt(i);
                }
                activeModifiers[i].Tick();
            }
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();
            if (activeModifiers.Count == 0)
                return;

            float xPosition = UI.screenWidth - 220f;
            float yPosition = 15f;

            // Toggle button
            Rect toggleRect = new Rect(xPosition, yPosition, 220f, 24f);
            if (Widgets.ButtonText(toggleRect, showModifiers ? "Hide Modifiers" : "Show Modifiers"))
            {
                showModifiers = !showModifiers;
            }

            if (!showModifiers)
                return;

            float totalHeight = 24f + 5f;

            for (int i = 0; i < activeModifiers.Count; i++)
            {
                totalHeight += iconSize.y + padding * 2;
                string explanation = activeModifiers[i].GetModifierExplanation();
                float textHeight = Text.CalcHeight(explanation, 200f);
                totalHeight += textHeight + 10f;
            }

            Rect panelRect = new Rect(xPosition, yPosition, 220f, totalHeight);
            Widgets.DrawBoxSolid(panelRect, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            Widgets.DrawBox(panelRect, 1);


            if (Widgets.ButtonText(toggleRect, showModifiers ? "Hide Modifiers" : "Show Modifiers"))
            {
                showModifiers = !showModifiers;
            }

            float currentY = yPosition + 24f + padding;

            for (int i = 0; i < activeModifiers.Count; i++)
            {
                MapModifier modifier = activeModifiers[i];
                Rect iconBgRect = new Rect(xPosition + padding, currentY, iconSize.x, iconSize.y);
                Widgets.DrawBoxSolid(iconBgRect, modifier.ModifierColor);
                Widgets.DrawBox(iconBgRect, 1);

                GUI.DrawTexture(iconBgRect, modifier.GetModifierTexture());
                string name = modifier.GetModifierName();
                Rect nameRect = new Rect(xPosition + iconSize.x + padding * 2, currentY,
                                       150f, Text.LineHeight);
                Widgets.Label(nameRect, name);
                currentY += iconSize.y + padding;
                string explanation = modifier.GetModifierExplanation();
                float textHeight = Text.CalcHeight(explanation, 200f - padding * 2);
                Rect textRect = new Rect(xPosition + padding, currentY, 200f - padding * 2, textHeight);
                Widgets.Label(textRect, explanation);
                currentY += textHeight + 10f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeModifiers, "activeModifiers", LookMode.Deep);
            Scribe_Values.Look(ref showModifiers, "showModifiers", true);
        }
    }
}

