using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicAndMyths
{
    public class Building_TurretGun_Controlled : Building_TurretGun
    {
        private Building_FireControlCenter controlCenter;
        private bool scheduledFirePending = false;
        private LocalTargetInfo scheduledTarget;
        public int assignedGroupId = -1;

        public float LeadingAccuracy = 0.8f;

        public Building_FireControlCenter ControlCenter => controlCenter;


        protected TurretBatteryGroup _TurretGroup;
        public TurretBatteryGroup TurretGroup
        {
            get => _TurretGroup;
        }


        public override Color DrawColor
        {
            get
            {
                if (_TurretGroup != null)
                {
                    return _TurretGroup.Color;
                }

                return Color.white;
            }
        }

        protected override bool CanSetForcedTarget => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                FindAndRegisterWithControlCenter();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            controlCenter?.UnregisterTurret(this);
            base.DeSpawn(mode);
        }

        private void FindAndRegisterWithControlCenter()
        {
            var controlCenters = Map.listerBuildings.AllBuildingsColonistOfClass<Building_FireControlCenter>()
                .OrderBy(cc => Vector3.Distance(Position.ToVector3(), cc.Position.ToVector3()));

            foreach (var cc in controlCenters)
            {
                if (cc.RegisterTurret(this))
                {
                    break;
                }
            }
        }

        public void SetControlCenter(Building_FireControlCenter center)
        {
            controlCenter = center;
        }

        public void SetAssignedGroup(TurretBatteryGroup batteryGroup)
        {
            if (batteryGroup == null)
            {
                assignedGroupId = -1;
                this._TurretGroup = null;
            }
            else
            {
                this._TurretGroup = batteryGroup;
                assignedGroupId = batteryGroup.GroupId;
            }
        }

        new public void TryStartShootSomething(bool canBeginBurstImmediately)
        {
            if (controlCenter != null && !scheduledFirePending)
            {
                return;
            }

            if (scheduledFirePending)
            {
                currentTargetInt = scheduledTarget;
                scheduledFirePending = false;
                scheduledTarget = LocalTargetInfo.Invalid;
            }

            base.TryStartShootSomething(canBeginBurstImmediately);
        }

        public void ExecuteScheduledFire(LocalTargetInfo target)
        {
            scheduledTarget = target;
            scheduledFirePending = true;
            TryStartShootSomething(true);
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            if (controlCenter != null)
            {
                controlCenter.OrderCoordinatedAttack(targ);
            }
            else
            {
                base.OrderAttack(targ);
            }
        }

        public void DirectOrderAttack(LocalTargetInfo targ)
        {
            base.OrderAttack(targ);
        }

        public override LocalTargetInfo TryFindNewTarget()
        {
            LocalTargetInfo baseTarget = base.TryFindNewTarget();
            if (!baseTarget.IsValid || !baseTarget.HasThing)
                return baseTarget;

            Pawn targetPawn = baseTarget.Thing as Pawn;
            if (targetPawn?.pather?.Moving == true)
            {
                Vector3 predictedPosition = CalculateLeadPosition(targetPawn);
                if (predictedPosition != Vector3.zero)
                {
                    IntVec3 leadCell = predictedPosition.ToIntVec3();
                    if (leadCell.InBounds(Map) && GenSight.LineOfSight(Position, leadCell, Map))
                    {
                        return new LocalTargetInfo(leadCell);
                    }
                }
            }
            return baseTarget;
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            if (phase == DrawPhase.Draw && CurrentTarget.IsValid && CurrentTarget.HasThing)
            {
                Pawn targetPawn = CurrentTarget.Thing as Pawn;
                if (targetPawn?.pather?.Moving == true)
                {
                    Vector3 predictedPosition = CalculateLeadPosition(targetPawn);
                    if (predictedPosition != Vector3.zero)
                    {
                        Vector3 currentPos = targetPawn.TrueCenter();
                        GenDraw.DrawLineBetween(currentPos, predictedPosition);
                    }
                }
            }
        }

        private Vector3 CalculateLeadPosition(Pawn target)
        {
            if (target?.pather?.curPath == null || target.pather.curPath.NodesLeftCount <= 1)
                return Vector3.zero;

            Vector3 currentPos = target.TrueCenter();
            Vector3 velocity = GetTargetVelocity(target);
            if (velocity.magnitude < 0.1f)
                return Vector3.zero;

            float projectileSpeed = GetProjectileSpeed();
            if (projectileSpeed <= 0)
                return Vector3.zero;

            float distance = Vector3.Distance(Position.ToVector3(), currentPos);
            float timeToTarget = distance / projectileSpeed;
            Vector3 predictedPos = currentPos + velocity * timeToTarget * LeadingAccuracy;

            return predictedPos;
        }

        private Vector3 GetTargetVelocity(Pawn target)
        {
            if (target.pather?.curPath == null || target.pather.curPath.NodesLeftCount <= 1)
                return Vector3.zero;

            IntVec3 nextNode = target.pather.nextCell;
            Vector3 direction = (nextNode.ToVector3() - target.Position.ToVector3()).normalized;
            float speed = target.GetStatValue(StatDefOf.MoveSpeed);
            return direction * speed;
        }

        private float GetProjectileSpeed()
        {
            if (AttackVerb?.verbProps?.defaultProjectile?.projectile != null)
            {
                return AttackVerb.verbProps.defaultProjectile.projectile.speed;
            }
            return 30f;
        }

        public void ResetForcedTarget()
        {
            this.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0)
            {
                this.TryStartShootSomething(false);
            }
        }

        public override string GetInspectString()
        {
            var sb = new System.Text.StringBuilder(base.GetInspectString());

            if (sb.Length > 0) sb.AppendLine();

            if (controlCenter != null)
            {
                sb.AppendLine($"Control center: {controlCenter.LabelCap}");
                if (assignedGroupId != -1)
                {
                    var group = controlCenter.GetAllGroups().FirstOrDefault(g => g.GroupId == assignedGroupId);
                    sb.AppendLine($"Battery group: {group?.Name ?? "Unknown"}");
                }
                else
                {
                    sb.AppendLine("Battery group: Unassigned");
                }
            }
            else
            {
                sb.AppendLine("Control center: None");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (controlCenter != null)
            {
                if (assignedGroupId != -1)
                {
                    var group = controlCenter.GetAllGroups().FirstOrDefault(g => g.GroupId == assignedGroupId);
                    if (group != null && group.Turrets.Count > 1)
                    {
                        yield return new Command_BatteryTarget(group, this);
                    }
                }

                yield return new Command_Action
                {
                    defaultLabel = "Assign to group",
                    defaultDesc = "Assign this turret to a battery group",
                    icon = TexCommand.Attack,
                    action = () => Find.WindowStack.Add(new FloatMenu(GetGroupAssignmentOptions()))
                };

                if (TurretGroup != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Unassign from group",
                        defaultDesc = "Remove this turret from its current group",
                        icon = TexCommand.CannotShoot,
                        action = () => controlCenter.RemoveTurretFromGroup(TurretGroup, this)
                    };
                }
            }
        }

        private List<FloatMenuOption> GetGroupAssignmentOptions()
        {
            var options = new List<FloatMenuOption>();

            foreach (var group in controlCenter.GetAllGroups())
            {
                options.Add(new FloatMenuOption(group.Name, () => controlCenter.AssignTurretToGroup(group, this)));
            }

            options.Add(new FloatMenuOption("Create new group", () =>
            {
                var newGroupId = controlCenter.CreateNewGroup();
                if (newGroupId != null)
                {
                    controlCenter.AssignTurretToGroup(newGroupId, this);
                }
            }));

            return options;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref controlCenter, "controlCenter");
            Scribe_Values.Look(ref scheduledFirePending, "scheduledFirePending");
            Scribe_Values.Look(ref assignedGroupId, "assignedGroupId", -1);
            Scribe_TargetInfo.Look(ref scheduledTarget, "scheduledTarget");
        }
    }
}
