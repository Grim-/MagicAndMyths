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

            float yPosition = 15f;
            float xPosition = UI.screenWidth - iconSize.x - padding * 2 - 15f;

            for (int i = 0; i < activeModifiers.Count; i++)
            {
                MapModifier modifier = activeModifiers[i];
                Rect boxRect = new Rect(xPosition, yPosition, iconSize.x + padding * 2, iconSize.y + padding * 2);

                Widgets.DrawBoxSolid(boxRect, modifier.ModifierColor);

                Widgets.DrawBox(boxRect, 1);

                Rect iconRect = new Rect(xPosition + padding, yPosition + padding, iconSize.x, iconSize.y);
                GUI.DrawTexture(iconRect, modifier.GetModifierTexture());

                if (Mouse.IsOver(boxRect))
                {
                    Widgets.DrawHighlight(boxRect);
                    TooltipHandler.TipRegion(boxRect, modifier.GetModifierExplanation());
                }

                yPosition += boxRect.height + 5f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeModifiers, "activeModifiers", LookMode.Deep);
        }

    }
}

