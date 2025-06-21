using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class Building_FireControlCenter : Building
    {
        private List<Building_TurretGun_Controlled> controlledTurrets = new List<Building_TurretGun_Controlled>();
        private Dictionary<int, TurretBatteryGroup> batteryGroups = new Dictionary<int, TurretBatteryGroup>();

        private List<int> batteryGroupWorkingKeys = new List<int>();
        private List<TurretBatteryGroup> batteryGroupWorkingValues = new List<TurretBatteryGroup>();

        private int nextGroupId = 1;
        public float controlRange = 50f;
        public int maxControlledTurrets = 20;
        public int maxGroups = 8;
        public bool coordinatedFiring = true;
        public float groupStaggerDelay = 0.5f;
        public float turretStaggerDelay = 0.1f;
        public bool autoGrouping = true;

        private int nextGroupFireTick = 0;
        private Queue<ScheduledBarrage> scheduledBarrages = new Queue<ScheduledBarrage>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                RegisterWithTurrets();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            ReleaseAllTurrets();
            base.DeSpawn(mode);
        }

        public override void Tick()
        {
            base.Tick();

            ProcessScheduledBarrages();

            if (this.IsHashIntervalTick(60))
            {
                RefreshControlledTurrets();
                OrganizeBatteryGroups();
            }
        }

        private void RegisterWithTurrets()
        {
            var availableTurrets = Map.listerBuildings.AllBuildingsColonistOfClass<Building_TurretGun_Controlled>()
                .Where(t => t.ControlCenter == null && Vector3.Distance(Position.ToVector3(), t.Position.ToVector3()) <= controlRange)
                .Take(maxControlledTurrets);

            foreach (var turret in availableTurrets)
            {
                RegisterTurret(turret);
            }
        }

        public bool RegisterTurret(Building_TurretGun_Controlled turret)
        {
            if (controlledTurrets.Count >= maxControlledTurrets) 
                return false;

            if (Vector3.Distance(Position.ToVector3(), turret.Position.ToVector3()) > controlRange) 
                return false;

            controlledTurrets.Add(turret);
            turret.SetControlCenter(this);

            if (autoGrouping && turret.assignedGroupId == -1)
            {
                AutoAssignTurretToGroup(turret);
            }

            return true;
        }

        public void UnregisterTurret(Building_TurretGun_Controlled turret)
        {
            controlledTurrets.Remove(turret);
            turret.SetControlCenter(null);

            if (turret.assignedGroupId != -1)
            {
                RemoveTurretFromGroup(turret.TurretGroup, turret);
            }
        }

        private void ReleaseAllTurrets()
        {
            foreach (var turret in controlledTurrets.ToList())
            {
                UnregisterTurret(turret);
            }
        }

        private void RefreshControlledTurrets()
        {
            for (int i = controlledTurrets.Count - 1; i >= 0; i--)
            {
                var turret = controlledTurrets[i];
                if (turret.Destroyed || !turret.Spawned ||
                    Vector3.Distance(Position.ToVector3(), turret.Position.ToVector3()) > controlRange)
                {
                    UnregisterTurret(turret);
                }
            }
        }

        private void OrganizeBatteryGroups()
        {
            if (!autoGrouping) 
                return;

            var unassignedTurrets = controlledTurrets.Where(t => t.Active && t.assignedGroupId == -1).ToList();
            var groupedTurrets = unassignedTurrets.GroupBy(t => t.def.defName).ToList();

            foreach (var group in groupedTurrets)
            {
                var groupId = GetOrCreateGroupByType(group.Key);
                foreach (var turret in group)
                {
                    AssignTurretToGroup(groupId, turret);
                }
            }
        }

        private void AutoAssignTurretToGroup(Building_TurretGun_Controlled turret)
        {
            var groupId = GetOrCreateGroupByType(turret.def.defName);
            AssignTurretToGroup(groupId, turret);
        }

        private TurretBatteryGroup GetOrCreateGroupByType(string defName)
        {
            var existingGroup = batteryGroups.Values.FirstOrDefault(g => g.DefName == defName);
            if (existingGroup != null)
            {
                return existingGroup;
            }

            return CreateNewGroup(defName);
        }

        public TurretBatteryGroup CreateNewGroup(string name = null)
        {
            if (batteryGroups.Count >= maxGroups) 
                return null;
            var groupId = nextGroupId++;
            var groupName = name ?? $"Group { NameGenerator.GenerateName(RulePackDefOf.NamerArtWeaponGun)}";
            TurretBatteryGroup group = new TurretBatteryGroup(groupId, groupName, null, new Color(Rand.Value, Rand.Value, Rand.Value));
            batteryGroups[groupId] = group;
            return group;
        }

        public bool AssignTurretToGroup(TurretBatteryGroup group, Building_TurretGun_Controlled turret)
        {
            if (!batteryGroups.ContainsKey(group.GroupId))
                return false;

            if (turret.TurretGroup != null)
            {
                RemoveTurretFromGroup(turret.TurretGroup, turret);
            }

            batteryGroups[group.GroupId].AddTurret(turret);
            return true;
        }

        public void RemoveTurretFromGroup(TurretBatteryGroup group, Building_TurretGun_Controlled turret)
        {
            if (batteryGroups.ContainsKey(group.GroupId))
            {
                batteryGroups[group.GroupId].RemoveTurret(turret);

                if (batteryGroups[group.GroupId].Turrets.Count == 0)
                {
                    batteryGroups.Remove(group.GroupId);
                }
            }
            turret.SetAssignedGroup(null);
        }

        public void DeleteGroup(int groupId)
        {
            if (!batteryGroups.ContainsKey(groupId)) return;

            var turretsToUnassign = batteryGroups[groupId].Turrets.ToList();
            foreach (var turret in turretsToUnassign)
            {
                turret.SetAssignedGroup(null);
                if (autoGrouping)
                {
                    AutoAssignTurretToGroup(turret);
                }
            }

            batteryGroups.Remove(groupId);
        }

        public void RenameGroup(int groupId, string newName)
        {
            if (batteryGroups.ContainsKey(groupId))
            {
                batteryGroups[groupId].Name = newName;
            }
        }

        public List<TurretBatteryGroup> GetAllGroups()
        {
            return batteryGroups.Values.ToList();
        }

        public void OrderCoordinatedAttack(LocalTargetInfo target)
        {
            if (!coordinatedFiring)
            {
                foreach (var turret in controlledTurrets)
                {
                    turret.ExecuteScheduledFire(target);
                }
                return;
            }

            ScheduleCoordinatedBarrage(target);
        }

        private void ScheduleCoordinatedBarrage(LocalTargetInfo target)
        {
            int currentTick = Find.TickManager.TicksGame;
            var activeGroups = batteryGroups.Values.Where(g => g.Turrets.Any(t => t.Active)).ToList();

            for (int groupIndex = 0; groupIndex < activeGroups.Count; groupIndex++)
            {
                var group = activeGroups[groupIndex];
                int groupFireTick = currentTick + (int)(groupIndex * groupStaggerDelay * 60);

                for (int turretIndex = 0; turretIndex < group.Turrets.Count; turretIndex++)
                {
                    var turret = group.Turrets[turretIndex];
                    if (!turret.Active) continue;

                    int turretFireTick = groupFireTick + (int)(turretIndex * turretStaggerDelay * 60);

                    scheduledBarrages.Enqueue(new ScheduledBarrage
                    {
                        turret = turret,
                        target = target,
                        fireTick = turretFireTick
                    });
                }
            }

            scheduledBarrages = new Queue<ScheduledBarrage>(scheduledBarrages.OrderBy(b => b.fireTick));
        }

        private void ProcessScheduledBarrages()
        {
            if (Find.TickManager.TicksGame >= nextGroupFireTick && scheduledBarrages.Count > 0)
            {
                int currentTick = Find.TickManager.TicksGame;

                while (scheduledBarrages.Count > 0 && scheduledBarrages.Peek().fireTick <= currentTick)
                {
                    var barrage = scheduledBarrages.Dequeue();
                    if (barrage.turret != null && !barrage.turret.Destroyed && barrage.turret.Spawned)
                    {
                        barrage.turret.ExecuteScheduledFire(barrage.target);
                    }
                }

                nextGroupFireTick = scheduledBarrages.Count > 0 ? scheduledBarrages.Peek().fireTick : int.MaxValue;
            }
        }

        public override string GetInspectString()
        {
            var sb = new System.Text.StringBuilder(base.GetInspectString());

            if (sb.Length > 0) sb.AppendLine();

            sb.AppendLine($"Controlled turrets: {controlledTurrets.Count}/{maxControlledTurrets}");
            sb.AppendLine($"Battery groups: {batteryGroups.Count}/{maxGroups}");
            sb.AppendLine($"Auto-grouping: {(autoGrouping ? "On" : "Off")}");
            sb.AppendLine($"Coordinated firing: {(coordinatedFiring ? "On" : "Off")}");

            if (scheduledBarrages.Count > 0)
            {
                sb.AppendLine($"Scheduled barrages: {scheduledBarrages.Count}");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (batteryGroups.Any(g => g.Value.Turrets.Any(t => t.Active)))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Target all batteries",
                    defaultDesc = "Target all active battery groups",
                    icon = TexButton.Save,
                    action = () =>
                    {
                        var activeGroups = batteryGroups.Values.Where(g => g.Turrets.Any(t => t.Active)).ToList();
                        if (activeGroups.Any())
                        {            
                            foreach (var item in activeGroups)
                            {
                                item.BeginTargeting(() =>
                                {
                                    foreach (var group in activeGroups)
                                    {
                                        group.OrderGroupAttack(group.GetLastTarget());
                                    }
                                });
          
                            }
  
                        }
                    }
                };

                foreach (var item in batteryGroups)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = $"Target {item.Value.Name} battery",
                        defaultDesc = $"Target all active turrets in {item.Value.Name} groups",
                        icon = TexButton.Save,
                        action = () =>
                        {
                            item.Value.BeginTargeting(() =>
                            {
                                item.Value.OrderGroupAttack(item.Value.GetLastTarget());
                            });               
                        }
                    };
                }
            }

            if (batteryGroups.Any(g => g.Value.Turrets.Any(t => t.Active)))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Stop all batteries",
                    defaultDesc = "Stop all active battery groups firing",
                    icon = TexButton.Save,
                    action = () =>
                    {
                        var activeGroups = batteryGroups.Values.Where(g => g.Turrets.Any(t => t.Active)).ToList();
                        if (activeGroups.Any())
                        {
                            foreach (var group in activeGroups)
                            {
                                group.OrderStopGroupAttack();
                            }
                        }
                    }
                };
            }

            yield return new Command_MultiAction
            {
                defaultLabel = "Auto-grouping",
                defaultDesc = "Automatically assign turrets to groups by type",
                icon = TexButton.SearchButton,
                actions = new List<Command_MultiAction.ActionData>()
                {
                    new Command_MultiAction.ActionData()
                    {
                        label = "Auto Grouping",
                        desc  = "Auto Grouping",
                        icon = TexButton.Infinity,
                        action = () => autoGrouping = !autoGrouping
                    },
                    new Command_MultiAction.ActionData()
                    {
                        label = "Coordinated firing",
                        desc  = "Coordinated firing",
                        icon = TexCommand.SquadAttack,
                        action = () => coordinatedFiring = !coordinatedFiring
                    },
                    new Command_MultiAction.ActionData()
                    {
                        label = "Create group",
                        desc  = "Create group",
                        icon = TexButton.Add,
                        action = () => CreateNewGroup()
                    },
                    new Command_MultiAction.ActionData()
                    {
                        label = "Manage groups",
                        desc = "Manage groups",
                        icon = TexCommand.ToggleVent,
                        action= () => Find.WindowStack.Add(new Dialog_ManageGroups(this))
                    }
                }
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref controlledTurrets, "controlledTurrets", LookMode.Reference);
            Scribe_Collections.Look(ref batteryGroups, "batteryGroups", LookMode.Value, LookMode.Deep, ref batteryGroupWorkingKeys, ref batteryGroupWorkingValues);
            Scribe_Values.Look(ref nextGroupId, "nextGroupId", 1);
            Scribe_Values.Look(ref coordinatedFiring, "coordinatedFiring", true);
            Scribe_Values.Look(ref autoGrouping, "autoGrouping", true);
            Scribe_Values.Look(ref groupStaggerDelay, "groupStaggerDelay", 0.5f);
            Scribe_Values.Look(ref turretStaggerDelay, "turretStaggerDelay", 0.1f);
        }
    }
}