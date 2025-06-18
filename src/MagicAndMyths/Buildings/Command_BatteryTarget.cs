using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Command_BatteryTarget : Command
    {
        private TurretBatteryGroup battery;
        private Building_TurretGun_Controlled turret;
        private List<TurretBatteryGroup> groupedBatteries;
        public bool drawRadius = true;
        public bool requiresAvailableVerb = true;
        public Verb verb;

        public Command_BatteryTarget(TurretBatteryGroup battery, Building_TurretGun_Controlled turret)
        {
            this.battery = battery;
            this.turret = turret;
            this.verb = turret?.AttackVerb;
            this.defaultLabel = "Battery Target";
            this.defaultDesc = "Target all turrets in this battery group";
            this.icon = TexButton.Add;
        }

        public override Color IconDrawColor
        {
            get
            {
                if (this.verb?.EquipmentSource != null)
                {
                    return this.verb.EquipmentSource.DrawColor;
                }
                return base.IconDrawColor;
            }
        }

        public override void MergeWith(Gizmo other)
        {
            base.MergeWith(other);
            Command_BatteryTarget command_BatteryTarget = other as Command_BatteryTarget;
            if (command_BatteryTarget == null)
            {
                Log.ErrorOnce("Tried to merge Command_BatteryTarget with unexpected type", 73406264);
                return;
            }
            if (this.groupedBatteries == null)
            {
                this.groupedBatteries = new List<TurretBatteryGroup>();
            }
            this.groupedBatteries.Add(command_BatteryTarget.battery);
            if (command_BatteryTarget.groupedBatteries != null)
            {
                this.groupedBatteries.AddRange(command_BatteryTarget.groupedBatteries);
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            battery.BeginTargeting(() =>
            {
                var target = battery.GetLastTarget();
                if (target.IsValid)
                {
                    battery.OrderGroupAttack(target);

                    if (groupedBatteries != null)
                    {
                        foreach (var groupedBattery in groupedBatteries)
                        {
                            groupedBattery.OrderGroupAttack(target);
                        }
                    }
                }
            });
        }

        public override void GizmoUpdateOnMouseover()
        {
            if (!this.drawRadius)
            {
                return;
            }
            if (verb != null)
            {
                verb.verbProps.DrawRadiusRing(verb.caster.Position);
            }
        }
    }
}
