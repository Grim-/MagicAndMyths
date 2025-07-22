using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace MagicAndMyths
{
    public class Command_MultiAction : Command
    {
        public class ActionData
        {
            public Action action;
            public string label;
            public string desc;
            public Texture icon;
            public bool disabled;
            public string disabledReason;
            public Color? iconColor;
        }

        public List<ActionData> actions = new List<ActionData>();
        private int lastClickedIndex = -1;

        public override string Label
        {
            get { return this.defaultLabel; }
        }

        public override string Desc
        {
            get { return this.defaultDesc; }
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            if (actions.Count == 0) return new GizmoResult(GizmoState.Clear, null);

            Text.Font = GameFont.Tiny;
            Color originalColor = GUI.color;
            Color color = Color.white;
            int hoveredIndex = -1;

            List<Rect> buttonRects = CalculateButtonRects(butRect);

            for (int i = 0; i < buttonRects.Count && i < actions.Count; i++)
            {
                if (Mouse.IsOver(buttonRects[i]))
                {
                    hoveredIndex = i;
                    if (!actions[i].disabled)
                    {
                        color = GenUI.MouseoverColor;
                    }
                    break;
                }
            }

            MouseoverSounds.DoRegion(butRect, SoundDefOf.Mouseover_Command);

            if (parms.highLight)
            {
                Widgets.DrawStrongHighlight(butRect.ExpandedBy(4f), null);
            }

            Material material = (parms.lowLight) ? TexUI.GrayscaleGUI : null;
            GUI.color = (parms.lowLight ? Command.LowLightBgColor : color);
            GenUI.DrawTextureWithMaterial(butRect, parms.shrunk ? this.BGTextureShrunk : this.BGTexture, material, default(Rect));
            GUI.color = originalColor;

            DrawDividers(butRect, buttonRects);
            DrawIcons(buttonRects, material, parms);

            int clickedIndex = -1;
            for (int i = 0; i < buttonRects.Count && i < actions.Count; i++)
            {
                if (Widgets.ButtonInvisible(buttonRects[i], true) && !actions[i].disabled)
                {
                    clickedIndex = i;
                    break;
                }
            }

            if (!parms.shrunk)
            {
                DrawLabels(buttonRects, parms);
            }

            if (hoveredIndex >= 0 && this.DoTooltip)
            {
                DrawTooltip(buttonRects[hoveredIndex], actions[hoveredIndex]);
            }

            Text.Font = GameFont.Small;

            if (clickedIndex >= 0)
            {
                if (actions[clickedIndex].disabled)
                {
                    if (!actions[clickedIndex].disabledReason.NullOrEmpty())
                    {
                        Messages.Message(actions[clickedIndex].disabledReason, MessageTypeDefOf.RejectInput, false);
                    }
                    return new GizmoResult(GizmoState.Mouseover, null);
                }

                lastClickedIndex = clickedIndex;
                return new GizmoResult(GizmoState.Interacted, Event.current);
            }
            else
            {
                if (hoveredIndex >= 0)
                {
                    return new GizmoResult(GizmoState.Mouseover, null);
                }
                return new GizmoResult(GizmoState.Clear, null);
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            if (lastClickedIndex >= 0 && lastClickedIndex < actions.Count)
            {
                actions[lastClickedIndex].action?.Invoke();
                lastClickedIndex = -1;
            }
        }

        private List<Rect> CalculateButtonRects(Rect butRect)
        {
            List<Rect> rects = new List<Rect>();
            int count = Mathf.Min(actions.Count, 4);

            switch (count)
            {
                case 1:
                    GridLayout grid1 = new GridLayout(butRect, 1, 1, 2f, 1f);
                    rects.Add(grid1.GetCellRect(0, 0));
                    break;

                case 2:
                    GridLayout grid2 = new GridLayout(butRect, 1, 2, 2f, 1f);
                    rects.Add(grid2.GetCellRect(0, 0));
                    rects.Add(grid2.GetCellRect(0, 1));
                    break;

                case 3:
                    GridLayout grid3 = new GridLayout(butRect, 2, 2, 2f, 1f);
                    rects.Add(grid3.GetCellRect(0, 0, 2, 1));
                    rects.Add(grid3.GetCellRect(0, 1));
                    rects.Add(grid3.GetCellRect(1, 1));
                    break;

                case 4:
                    GridLayout grid4 = new GridLayout(butRect, 2, 2, 2f, 1f);
                    rects.Add(grid4.GetCellRect(0, 0));
                    rects.Add(grid4.GetCellRect(1, 0));
                    rects.Add(grid4.GetCellRect(0, 1));
                    rects.Add(grid4.GetCellRect(1, 1));
                    break;
            }

            return rects;
        }

        private void DrawDividers(Rect butRect, List<Rect> buttonRects)
        {
            if (buttonRects.Count <= 1) return;

            GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            int count = buttonRects.Count;

            if (count >= 2)
            {
                float midY = butRect.y + butRect.height / 2f;
                GUI.DrawTexture(new Rect(butRect.x + 2f, midY - 0.5f, butRect.width - 4f, 1f), TexUI.GrayTextBG);
            }

            if (count >= 3)
            {
                float midX = butRect.x + butRect.width / 2f;
                float bottomY = butRect.y + butRect.height / 2f;
                GUI.DrawTexture(new Rect(midX - 0.5f, bottomY + 1f, 1f, butRect.height / 2f - 2f), TexUI.GrayTextBG);
            }

            if (count == 4)
            {
                float midX = butRect.x + butRect.width / 2f;
                float topY = butRect.y + 2f;
                GUI.DrawTexture(new Rect(midX - 0.5f, topY, 1f, butRect.height / 2f - 2f), TexUI.GrayTextBG);
            }

            GUI.color = Color.white;
        }

        private void DrawIcons(List<Rect> buttonRects, Material buttonMat, GizmoRenderParms parms)
        {
            for (int i = 0; i < buttonRects.Count && i < actions.Count; i++)
            {
                if (actions[i].icon != null)
                {
                    Rect iconRect = buttonRects[i].ContractedBy(4f);

                    GUI.color = actions[i].iconColor ?? Color.white;
                    if (actions[i].disabled)
                    {
                        GUI.color = GUI.color.SaturationChanged(0f);
                    }
                    if (parms.lowLight)
                    {
                        GUI.color = GUI.color.ToTransparent(0.6f);
                    }

                    if (!actions[i].label.NullOrEmpty())
                    {
                        GUI.color = GUI.color.ToTransparent(0.3f);
                    }

                    Widgets.DrawTextureFitted(iconRect, actions[i].icon, 0.85f);
                }
            }
            GUI.color = Color.white;
        }

        private void DrawLabels(List<Rect> buttonRects, GizmoRenderParms parms)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = parms.lowLight ? Command.LowLightLabelColor : Color.white;

            for (int i = 0; i < buttonRects.Count && i < actions.Count; i++)
            {
                if (!actions[i].label.NullOrEmpty())
                {
                    Rect labelRect = buttonRects[i].ContractedBy(1f);
                    GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
                    GUI.color = parms.lowLight ? Command.LowLightLabelColor : Color.white;
                    Widgets.Label(labelRect, actions[i].label);
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private void DrawTooltip(Rect rect, ActionData actionData)
        {
            TipSignal tip = actionData.desc;
            if (actionData.disabled && !actionData.disabledReason.NullOrEmpty())
            {
                tip.text += ("\n\n" + "DisabledCommand".Translate() + ": " + actionData.disabledReason).Colorize(ColorLibrary.RedReadable);
            }
            TooltipHandler.TipRegion(rect, tip);
        }

        public void AddAction(Action action, string label, string desc, Texture icon = null, bool disabled = false, string disabledReason = "", Color? iconColor = null)
        {
            if (actions.Count >= 4) return;

            actions.Add(new ActionData
            {
                action = action,
                label = label,
                desc = desc,
                icon = icon,
                disabled = disabled,
                disabledReason = disabledReason,
                iconColor = iconColor
            });
        }
    }
}