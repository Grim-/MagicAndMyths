using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyStatus : Gizmo
    {
        public Thing thing;
        public IEnergyProvider EnergyComp;
        public string customLabel;
        public Color barColor = new Color(0.8f, 0.8f, 0.2f);
        public bool LerpColor = true;
        public Color EmptyColor = Color.red;
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        // Add collapsed state
        protected bool collapsed = false;
        private static readonly float CollapsedHeight = 24f;
        private static readonly float ExpandedHeight = 75f;

        public Gizmo_EnergyStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            float height = collapsed ? CollapsedHeight : ExpandedHeight;
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), height);
            Rect innerRect = rect.ContractedBy(6f);

            // Draw background
            Widgets.DrawWindowBackground(rect);

            // Add collapse toggle button
            Rect toggleRect = new Rect(rect.x + rect.width - 24f, rect.y + 2f, 20f, 20f);
            if (Widgets.ButtonImage(toggleRect, collapsed ? TexButton.Collapse : TexButton.Collapse))
            {
                collapsed = !collapsed;
            }

            if (collapsed)
            {
                DrawCollapsed(innerRect, maxWidth, parms);
            }
            else
            {
                DrawExpanded(rect, innerRect, maxWidth, parms);
            }

            // Tooltip
            string tooltipText = $"Energy: {EnergyComp.Energy:F0} / {EnergyComp.MaxEnergy:F0}";
            TooltipHandler.TipRegion(rect, tooltipText);

            return new GizmoResult(GizmoState.Clear);
        }

        public virtual void DrawCollapsed(Rect innerRect, float maxWidth, GizmoRenderParms parms)
        {
            // Draw minimal info when collapsed
            Text.Font = GameFont.Tiny;
            Rect miniLabelRect = new Rect(innerRect.x, innerRect.y, innerRect.width - 26f, CollapsedHeight);
            Widgets.Label(miniLabelRect, $"{customLabel ?? thing.LabelCap}: {(EnergyComp.GetEnergyPercent() * 100f):F0}%");
        }

        public virtual void DrawExpanded(Rect rect, Rect innerRect, float maxWidth, GizmoRenderParms parms)
        {
            // Draw full interface when expanded
            // Label
            Rect labelRect = innerRect;
            labelRect.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, customLabel ?? thing.LabelCap);

            // Energy bar
            Rect barRect = innerRect;
            barRect.yMin = labelRect.yMax;
            Color lerpColored = Color.Lerp(EmptyColor, barColor, EnergyComp.GetEnergyPercent());
            var barTex = SolidColorMaterials.NewSolidColorTexture(lerpColored);
            Widgets.FillableBar(barRect, EnergyComp.GetEnergyPercent(), barTex, EmptyBarTex, false);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, $"{(EnergyComp.GetEnergyPercent() * 100f):F0}%");
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
