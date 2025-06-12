using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PortalEquipmentComp : CompProperties
    {
        public float aoeRadius = 4f;
        public float teleportCost = 50f;

        public CompProperties_PortalEquipmentComp()
        {
            compClass = typeof(Portal_EquipmentComp);
        }
    }

    public class Portal_EquipmentComp : ThingComp, IPortalDevice
    {
        public PortalMode mode = PortalMode.Single;
        public bool drawRadius = false;

        public CompProperties_PortalEquipmentComp Props => (CompProperties_PortalEquipmentComp)props;

        private Comp_Energy _Energy;
        public Comp_Energy Energy
        {
            get
            {
                if (_Energy == null)
                {
                    _Energy = this.parent.GetComp<Comp_Energy>();
                }

                return _Energy;
            }
        }
        private Pawn EquippingPawn
        {
            get
            {
                Pawn_ApparelTracker apparelTracker = parent.ParentHolder as Pawn_ApparelTracker;
                return apparelTracker?.pawn;
            }
        }

        public string GetActionLabel
        {
            get
            {
                return mode == PortalMode.Group
                    ? $"Return to a colony with nearby pawns (Radius: {Props.aoeRadius})"
                    : "Return to one of your colonies with a portal gate.";
            }
        }
        public string GetActionDescription
        {
            get
            {
                return mode == PortalMode.Group
                    ? $"Return to a colony with nearby pawns (Radius: {Props.aoeRadius})"
                    : "Return to one of your colonies with a portal gate.";
            }
        }
        public string GetTooltip
        {
            get
            {
                return mode == PortalMode.Group
                    ? $"Return to a colony with nearby pawns (Radius: {Props.aoeRadius})"
                    : "Return to one of your colonies with a portal gate.";
            }
        }
        public string GetModeTooltip
        {
           get
            {
                return CanTeleport() ?
                (mode == PortalMode.Group ? "Group Mode Active" : "Single Mode Active") :
                "Not enough energy";
            }
        }
        public Color GetModeTooltipColor
        {
            get
            {
               return Energy.HasEnough(Props.teleportCost) ? Color.white : Color.red;
            }
        }

        bool IPortalDevice.CanTeleport => CanTeleport();

        public string ActionLabel => GetActionLabel;

        public string ActionDescription => GetActionDescription;

        public string ModeTooltip => GetModeTooltip;

        public Color ModeTooltipColor => GetModeTooltipColor;


        public override void PostDraw()
        {
            base.PostDraw();
            if (mode == PortalMode.Group && EquippingPawn != null)
            {
                GenDraw.DrawRadiusRing(EquippingPawn.Position, Props.aoeRadius);
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var item in base.CompGetWornGizmosExtra())
            {
                yield return item;
            }

            yield return new Gizmo_PortalStatus
            {
                thing = parent,
                PortalDevice = this,
                customLabel = "Teleport Belt",
                barColor = new Color(0.2f, 0.6f, 0.9f),
                OnTeleportPressed = (Map map) =>
                {
                    if (mode == PortalMode.Single)
                    {
                        TeleportToMap(map);
                    }
                    else
                    {
                        TeleportGroupToMap(map);
                    }
                },
                OnToggleModePressed = () => ToggleMode()
            };
        }


        public bool CanTeleport()
        {
            return Energy != null ? Energy.HasEnough(Props.teleportCost) : true;
        }

        public void ToggleMode()
        {
            mode = mode == PortalMode.Single ? mode = PortalMode.Group : PortalMode.Single;
        }

        public bool IsValidColonyMap(Map map, out Building_PortalGate portalGate)
        {
            portalGate = (Building_PortalGate)map.listerBuildings.AllBuildingsColonistOfDef(MagicAndMythDefOf.Portal_GateLarge).FirstOrDefault();
            return portalGate != null;
        }

        public void TeleportToMap(Map targetMap, Building_PortalGate portal = null)
        {
            IntVec3 position = portal?.Position ?? targetMap.Center;
            if (!EquippingPawn.TransferToMap(position, targetMap, false))
            {
                Messages.Message("Failed to teleport", MessageTypeDefOf.RejectInput);
            }
        }

        public void TeleportGroupToMap(Map targetMap, Building_PortalGate portal = null)
        {
            if (EquippingPawn?.Map == null) return;

            IntVec3 targetPosition = portal?.Position ?? targetMap.Center;
            var pawnsInRadius = GenRadial.RadialDistinctThingsAround(EquippingPawn.Position, EquippingPawn.Map, Props.aoeRadius, true)
                .OfType<Pawn>()
                .Where(p => p.Faction == EquippingPawn.Faction && !p.Dead && !p.Downed)
                .ToList();

            int successCount = 0;
            foreach (var pawn in pawnsInRadius)
            {
                if (pawn.TransferToMap(targetPosition, targetMap, false))
                {
                    successCount++;
                }
            }

            if (successCount > 0)
            {
                Messages.Message($"Teleported {successCount} pawns", MessageTypeDefOf.PositiveEvent);

                if (Energy != null)
                {
                    Energy.TryUseEnergy(Props.teleportCost);
                }
            }
            else
            {
                Messages.Message("Failed to teleport any pawns", MessageTypeDefOf.RejectInput);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<PortalMode>(ref mode, "aoeMode");
        }
    }

}