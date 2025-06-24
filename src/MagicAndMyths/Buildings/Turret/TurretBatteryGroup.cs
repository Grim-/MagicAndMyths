using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class TurretBatteryGroup : IExposable, ILoadReferenceable
    {
        public int GroupId;
        public string Name;
        public string DefName;
        public List<Building_TurretGun_Controlled> Turrets = new List<Building_TurretGun_Controlled>();
        private LocalTargetInfo lastTarget;

        public Color Color = Color.red;

        public TurretBatteryGroup()
        {
            Turrets = new List<Building_TurretGun_Controlled>();
        }

        public TurretBatteryGroup(int groupId, string name, string defName = null, Color color = default(Color))
        {
            GroupId = groupId;
            Name = name;
            DefName = defName;
            Turrets = new List<Building_TurretGun_Controlled>();

            if (color != default(Color))
            {
                Color = color;
            }
            else Color = Color.white;
        }

        public void AddTurret(Building_TurretGun_Controlled turret)
        {
            if (!Turrets.Contains(turret))
            {
                Turrets.Add(turret);
                if (DefName == null && turret?.def?.defName != null)
                {
                    DefName = turret.def.defName;
                }
                turret.SetAssignedGroup(this);
            }
        }

        public void RemoveTurret(Building_TurretGun_Controlled turret)
        {
            Turrets.Remove(turret);
        }

        public void BeginTargeting(Action onComplete)
        {
            var activeTurret = Turrets.FirstOrDefault(t => t.Active && t.AttackVerb != null);
            if (activeTurret?.AttackVerb != null)
            {
                Find.Targeter.BeginTargeting(activeTurret.AttackVerb.targetParams,
                    target =>
                    {
                        lastTarget = target;
                        onComplete?.Invoke();
                    }, null, null, null);
            }
        }

        public void OrderGroupAttack(LocalTargetInfo target)
        {
            foreach (var turret in Turrets.Where(t => t.Active))
            {
                turret.DirectOrderAttack(target);
            }
        }

        public void OrderStopGroupAttack()
        {
            foreach (var turret in Turrets.Where(t => t.Active))
            {
                turret.ResetCurrentTarget();
                turret.ResetForcedTarget();
            }
        }

        public LocalTargetInfo GetLastTarget()
        {
            return lastTarget;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref GroupId, "groupId");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref DefName, "defName");
            Scribe_Collections.Look(ref Turrets, "turrets", LookMode.Reference);
            Scribe_TargetInfo.Look(ref lastTarget, "lastTarget");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Turrets = Turrets.Where(t => t != null).ToList();
            }
        }

        public string GetUniqueLoadID()
        {
            return "TurretBatteryGroup_" + Find.UniqueIDsManager.GetNextThingID();
        }
    }
}