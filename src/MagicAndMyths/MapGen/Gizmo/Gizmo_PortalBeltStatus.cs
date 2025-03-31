using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class Gizmo_PortalBeltStatus : Gizmo_EnergyStatus
    {
        public Portal_EquipmentComp PortalComp;

        private Texture2D _ToggleTexture;
        private Texture2D ToggleTexture
        {
            get
            {
                if (_ToggleTexture == null)
                {
                    _ToggleTexture = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower");
                }

                return _ToggleTexture;
            }
        }


        private Texture2D _TeleportTexture;
        private Texture2D TeleportTexture
        {
            get
            {
                if (_TeleportTexture == null)
                {
                    _TeleportTexture = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower");
                }

                return _TeleportTexture;
            }
        }


        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            // Draw base energy display
            base.GizmoOnGUI(topLeft, maxWidth, parms);
            float buttonSize = 24f;
            float padding = 4f;
            float totalWidth = GetWidth(maxWidth);

            // Toggle button (rightmost)
            Rect toggleRect = new Rect(
                topLeft.x + totalWidth - buttonSize - padding,
                topLeft.y + padding,
                buttonSize,
                buttonSize);

            // Teleport button (to the left of toggle)
            Rect teleportRect = new Rect(
                topLeft.x + totalWidth - (buttonSize * 2) - (padding * 2),
                topLeft.y + padding,
                buttonSize,
                buttonSize);

            bool hasEnoughEnergy = PortalComp.CanTeleport();
            Color buttonColor = PortalComp.GetModeTooltipColor;

            // Draw toggle button
            if (Widgets.ButtonImage(toggleRect, ToggleTexture, buttonColor, buttonColor * 0.8f))
            {
                if (hasEnoughEnergy)
                {
                    PortalComp.ToggleMode();
                }
            }
            TooltipHandler.TipRegion(toggleRect, PortalComp.GetModeTooltip);

            // Maps check for teleport button
            var validColonies = Find.Maps
                .Where(map => map.ParentFaction == Faction.OfPlayer &&
                       map.listerBuildings.ColonistsHaveBuilding(MagicAndMythDefOf.Portal_GateLarge))
                .ToList();

            // Draw teleport button
            if (validColonies.Any() && hasEnoughEnergy)
            {
                if (Widgets.ButtonImage(teleportRect, TexButton.Infinity, Color.white, Color.white * 0.8f))
                {
                    if (validColonies.Count == 1)
                    {
                        if (PortalComp.IsValidColonyMap(validColonies[0], out Building_PortalGate portal))
                        {
                            if (PortalComp.mode == PortalMode.Group)
                                PortalComp.TeleportGroupToMap(validColonies[0], portal);
                            else
                                PortalComp.TeleportToMap(validColonies[0], portal);
                        }
                    }
                    else
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (Map colony in validColonies)
                        {
                            options.Add(new FloatMenuOption(colony.Parent.Label, () =>
                            {
                                if (PortalComp.IsValidColonyMap(colony, out Building_PortalGate portal))
                                {
                                    if (PortalComp.mode == PortalMode.Group)
                                        PortalComp.TeleportGroupToMap(colony, portal);
                                    else
                                        PortalComp.TeleportToMap(colony, portal);
                                }
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
                string actionLabel = PortalComp.GetActionLabel;
                string actionDesc = PortalComp.GetActionDescription;
                TooltipHandler.TipRegion(teleportRect, actionDesc);
            }
            else
            {
                Widgets.ButtonImage(teleportRect, TexButton.Infinity, Color.red);
                TooltipHandler.TipRegion(teleportRect, !hasEnoughEnergy ?
                    "Not enough energy" : "No valid colonies with portals");
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
