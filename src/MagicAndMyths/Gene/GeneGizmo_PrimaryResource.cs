using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class GeneGizmo_PrimaryResource : GeneGizmo_BasicResource
    {
        private const float TotalPulsateTime = 0.85f;
        private const float IconSize = 16f;
        private const float IconSpacing = 18f;
        private const float IconMargin = 4f;

        private List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();
        private Gene_BasicResource basicResourceGene;

        protected override string Title
        {
            get
            {
                if (basicResourceGene?.ResourceDef != null)
                {
                    return basicResourceGene?.ResourceDef.label;
                }
                return base.Title;
            }
        }
        protected override float ValuePercent
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.ValuePercent;
                }

                return base.ValuePercent;
            }
        }

        protected override string BarLabel
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return $"{basicResource.ValueForDisplay} / {basicResource.MaxForDisplay}";
                }

                return base.BarLabel;
            }
        }

        protected override Color BarColor
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.ResourceDef.barColor;
                }

                return base.BarColor;
            }
        }

        protected override Color BarHighlightColor
        {
            get
            {
                if (gene is Gene_BasicResource basicResource)
                {
                    return basicResource.ResourceDef.barHighlightColor;
                }

                return base.BarHighlightColor;
            }
        }
        public GeneGizmo_PrimaryResource(Gene_BasicResource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor) : base(gene, drainGenes, barColor, barHighlightColor)
        {
            if (gene == null)
            {
                Log.Error("GeneGizmo_PrimaryResourceWithToggles created with null gene");
                return;
            }
            basicResourceGene = gene;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            var additionalResources = GetAdditionalResources();
            if (additionalResources.Any())
            {
                DrawAdditionalResourceIcons(topLeft, maxWidth, additionalResources);
            }

            return result;
        }

        private List<ResourceData> GetAdditionalResources()
        {
            var additionalResources = new List<ResourceData>();

            if (basicResourceGene.AdditionalResources != null)
            {
                foreach (var extraResource in basicResourceGene.AdditionalResources)
                {
                    if (extraResource.Value != null)
                    {
                        var resourceData = basicResourceGene.GetAdditionalResource(extraResource.Value.resourceDef);
                        if (resourceData != null)
                        {
                            additionalResources.Add(resourceData);
                        }
                    }
                }
            }

            return additionalResources;
        }

        private void DrawAdditionalResourceIcons(Vector2 topLeft, float maxWidth, List<ResourceData> additionalResources)
        {
            float gizmoWidth = GetWidth(maxWidth);
            float iconY = topLeft.y + IconMargin;
            float startX = topLeft.x + gizmoWidth - IconMargin - (additionalResources.Count * IconSpacing);

            for (int i = 0; i < additionalResources.Count; i++)
            {
                var resource = additionalResources[i];
                if (resource?.resourceDef == null)
                    continue;

                float iconX = startX + (i * IconSpacing);
                Rect iconRect = new Rect(iconX, iconY, IconSize, IconSize);

                bool isVisible = basicResourceGene.IsAdditionalResourceVisible(resource.resourceDef);
                Color iconColor = isVisible ? resource.resourceDef.barColor : Color.gray;

                GUI.color = iconColor;
                GUI.DrawTexture(iconRect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                if (Widgets.ButtonImage(iconRect, TexButton.Add))
                {
                    basicResourceGene.ToggleAdditionalResourceVisibility(resource.resourceDef);
                }

                if (Mouse.IsOver(iconRect))
                {
                    string tooltip = $"{resource.resourceDef.label}: {resource.currentValue:F0}/{resource.maxValue:F0}\nClick to {(isVisible ? "hide" : "show")}";
                    TooltipHandler.TipRegion(iconRect, tooltip);
                }
            }
        }

        protected override string GetTooltip()
        {
            if (basicResourceGene?.ResourceDef == null)
                return string.Empty;

            var primaryResource = basicResourceGene.ResourceDef;
            string text = $"{primaryResource.label.CapitalizeFirst()}: {basicResourceGene.ValueForDisplay} / {basicResourceGene.MaxForDisplay}\n";

            if (primaryResource.regenStat != null)
            {
                string regen = $"\nRegenerates {primaryResource.RegenStatValue(basicResourceGene.Pawn)} {primaryResource.label.CapitalizeFirst()} every {primaryResource.RegenTicksValue(basicResourceGene.Pawn).ToStringTicksToPeriod()} ticks";
                text += regen;
            }

            if (!basicResourceGene.def.resourceDescription.NullOrEmpty())
            {
                text += $"\n\n{basicResourceGene.def.resourceDescription.Formatted(basicResourceGene.pawn.Named("PAWN")).Resolve()}";
            }

            var additionalResources = GetAdditionalResources();
            if (additionalResources.Any())
            {
                text += "\n\nAdditional Resources:";
                foreach (var resource in additionalResources)
                {
                    text += $"\n• {resource.resourceDef.label}: {resource.currentValue:F0}/{resource.maxValue:F0}";
                }
            }

            return text;
        }
    }
}
