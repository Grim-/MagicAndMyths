using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_Teleporter : HediffCompProperties_EnergyComp
    {

        public HediffCompProperties_Teleporter()
        {
            compClass = typeof(HediffComp_Teleporter);
        }
    }

    public class HediffComp_Teleporter : HediffComp_EnergyComp, IPortalDevice
    {
        public PortalMode mode = PortalMode.Single;
        public bool drawRadius = false;
        new public HediffCompProperties_Teleporter Props => (HediffCompProperties_Teleporter)props;
        private float energy;

        public string GetActionLabel =>
            mode == PortalMode.Group
                ? $"Return to a colony with nearby pawns (Radius: {Props.aoeRadius})"
                : "Return to one of your colonies with a portal gate.";

        public string GetActionDescription => GetActionLabel;
        public string GetTooltip => GetActionLabel;

        public string GetModeTooltip =>
            CanTeleport()
                ? (mode == PortalMode.Group ? "Group Mode Active" : "Single Mode Active")
                : "Not enough energy";

        public Color GetModeTooltipColor =>
            HasEnough(Props.teleportCost) ? Color.white : Color.red;

        bool IPortalDevice.CanTeleport => CanTeleport();

        public string ActionLabel => GetActionLabel;

        public string ActionDescription => GetActionDescription;

        public string ModeTooltip => GetModeTooltip;

        public Color ModeTooltipColor => GetModeTooltipColor;


        public override string CompDescriptionExtra => base.CompDescriptionExtra + $"Your bioelectric field recharges {RegenPerHour} per hour.";

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (mode == PortalMode.Group && parent.pawn != null)
            {
                GenDraw.DrawRadiusRing(parent.pawn.Position, Props.aoeRadius);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (this.Pawn.Drafted)
            {
                yield return new Gizmo_PortalStatus
                {
                    thing = parent.pawn,
                    PortalDevice = this,
                    EnergyComp = this,
                    customLabel = "Teleport Chip",
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
                    OnToggleModePressed = () =>  ToggleMode()
                };
            }
        }

        public bool CanTeleport() => HasEnough(Props.teleportCost);

        public void ToggleMode()
        {
            mode = mode == PortalMode.Single ? PortalMode.Group : PortalMode.Single;
        }

        public bool IsValidColonyMap(Map map, out Building_PortalGate portalGate)
        {
            portalGate = (Building_PortalGate)map.listerBuildings.AllBuildingsColonistOfDef(MagicAndMythDefOf.Portal_GateLarge).FirstOrDefault();
            return portalGate != null;
        }

        public void TeleportToMap(Map targetMap, Building_PortalGate portal = null)
        {
            if (parent.pawn == null) 
                return;

            IntVec3 position = portal?.Position ?? targetMap.Center;
            if (parent.pawn.TransferToMap(position, targetMap, false))
            {
                TryUseEnergy(Props.teleportCost);             
            }
            else
            {
                Messages.Message("Failed to teleport", MessageTypeDefOf.RejectInput);
            }
        }

        public void TeleportGroupToMap(Map targetMap, Building_PortalGate portal = null)
        {
            if (parent.pawn?.Map == null) return;

            IntVec3 targetPosition = portal?.Position ?? targetMap.Center;
            var pawnsInRadius = GenRadial.RadialDistinctThingsAround(parent.pawn.Position, parent.pawn.Map, Props.aoeRadius, true)
                .OfType<Pawn>()
                .Where(p => p.Faction == parent.pawn.Faction && !p.Dead && !p.Downed)
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
                TryUseEnergy(Props.teleportCost);
            }
            else
            {
                Messages.Message("Failed to teleport any pawns", MessageTypeDefOf.RejectInput);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref mode, "aoeMode");
            Scribe_Values.Look(ref energy, "energy", 0f);
        }
    }

}