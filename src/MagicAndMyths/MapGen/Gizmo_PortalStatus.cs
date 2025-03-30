using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class Gizmo_PortalStatus : Gizmo_EnergyStatus
    {
        public IPortalDevice PortalDevice;
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

        public Action<Map> OnTeleportPressed;
        public Action OnToggleModePressed;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            base.GizmoOnGUI(topLeft, maxWidth, parms);

            float buttonSize = 24f;
            float padding = 4f;
            float totalWidth = GetWidth(maxWidth);

            // Adjust button positions based on collapsed state
            float buttonY = collapsed ? 0f : padding;

            Rect toggleRect = new Rect(
                topLeft.x + totalWidth - buttonSize - padding,
                topLeft.y + buttonY,
                buttonSize,
                buttonSize);

            Rect teleportRect = new Rect(
                topLeft.x + totalWidth - (buttonSize * 2) - (padding * 2),
                topLeft.y + buttonY,
                buttonSize,
                buttonSize);

            if (Widgets.ButtonImage(toggleRect, ToggleTexture, PortalDevice.ModeTooltipColor))
            {
                OnToggleModePressed?.Invoke();
            }
            TooltipHandler.TipRegion(toggleRect, PortalDevice.ModeTooltip);

            var validColonies = Find.Maps
                .Where(map => map.ParentFaction == Faction.OfPlayer &&
                       map.listerBuildings.ColonistsHaveBuilding(MagicAndMythDefOf.Portal_GateLarge))
                .ToList();

            // Draw teleport button
            if (validColonies.Any() && PortalDevice.CanTeleport)
            {
                if (Widgets.ButtonImage(teleportRect, TeleportTexture, Color.white))
                {
                    if (validColonies.Count == 1)
                    {
                        OnTeleportPressed?.Invoke(validColonies[0]);
                    }
                    else
                    {
                        List<FloatMenuOption> options = validColonies
                            .Select(map => new FloatMenuOption(map.Parent.Label, () => OnTeleportPressed?.Invoke(map)))
                            .ToList();
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
                TooltipHandler.TipRegion(teleportRect, PortalDevice.ActionDescription);
            }
            else
            {
                Widgets.ButtonImage(teleportRect, TeleportTexture, Color.gray);
                TooltipHandler.TipRegion(teleportRect, !PortalDevice.CanTeleport ?
                    "Not enough energy" : "No valid colonies with portals");
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
