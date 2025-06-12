using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using static Verse.DamageWorker;

namespace MagicAndMyths
{
    public abstract class DamageWorkerCompProperties
    {
        public Type compClass;

        public DamageWorkerCompProperties()
        {
        
        }
    }

    public abstract class DamageWorkerComp
    {
        public DamageWorkerCompProperties props;

        public virtual void Initialize(DamageWorkerCompProperties props)
        {
            this.props = props;
        }

        // Called before any damage calculations - can modify damage info
        public virtual DamageInfo PreApply(DamageInfo dinfo, Thing thing)
        {
            return dinfo;
        }

        // Called after damage is applied but target is still alive
        public virtual DamageResult PostApply(DamageInfo dinfo, Thing thing, DamageResult result)
        {
            return result;
        }

        // Called when the damage kills the target
        public virtual void OnKilled(DamageInfo dinfo, Thing thing, DamageResult result)
        {
        }

        public virtual bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            return true;
        }
    }

    public class ExtendedDamageDef : DamageDef
    {
        public List<DamageWorkerCompProperties> comps = new List<DamageWorkerCompProperties>();

        [Unsaved]
        private List<DamageWorkerComp> cachedComps;

        public List<DamageWorkerComp> WorkerComps
        {
            get
            {
                if (cachedComps == null)
                {
                    cachedComps = new List<DamageWorkerComp>();
                    if (comps != null)
                    {
                        foreach (var compProps in comps)
                        {
                            try
                            {
                                var comp = (DamageWorkerComp)Activator.CreateInstance(compProps.compClass);
                                comp.Initialize(compProps);
                                cachedComps.Add(comp);
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Failed to create damage worker comp {compProps.compClass}: {e}");
                            }
                        }
                    }
                }
                return cachedComps;
            }
        }
    }

    public class DamageWorker_Extended : DamageWorker_AddInjury
    {
        ExtendedDamageDef ExtendedDef => (ExtendedDamageDef)def;

        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            Log.Message("DamageWorker_Extended Apply");

            // Pre-damage hooks - can modify damage info
            foreach (var comp in ExtendedDef.WorkerComps)
            {
                if (comp.ShouldApply(dinfo, victim))
                {
                    dinfo = comp.PreApply(dinfo, victim);
                }
            }

            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
            if (victim.SpawnedOrAnyParentSpawned)
            {
                ImpactSoundUtility.PlayImpactSound(victim, dinfo.Def.impactSoundType, victim.MapHeld);
            }
            if (victim.def.useHitPoints && dinfo.Def.harmsHealth)
            {
                float num = dinfo.Amount;
                if (victim.def.category == ThingCategory.Building)
                {
                    num *= dinfo.Def.buildingDamageFactor;
                    if (victim.def.passability == Traversability.Impassable)
                    {
                        num *= dinfo.Def.buildingDamageFactorImpassable;
                    }
                    else
                    {
                        num *= dinfo.Def.buildingDamageFactorPassable;
                    }
                    if (dinfo.Def.scaleDamageToBuildingsBasedOnFlammability)
                    {
                        num *= Mathf.Max(0.05f, victim.GetStatValue(StatDefOf.Flammability, true, -1));
                    }
                    Pawn pawn;
                    if ((pawn = (dinfo.Instigator as Pawn)) != null && pawn.IsShambler)
                    {
                        num *= 1.5f;
                    }
                    if (ModsConfig.BiotechActive && dinfo.Instigator != null && (dinfo.WeaponBodyPartGroup != null || (dinfo.Weapon != null && dinfo.Weapon.IsMeleeWeapon)) && victim.def.IsDoor)
                    {
                        num *= dinfo.Instigator.GetStatValue(StatDefOf.MeleeDoorDamageFactor, true, -1);
                    }
                }
                if (victim.def.category == ThingCategory.Plant)
                {
                    num *= dinfo.Def.plantDamageFactor;
                }
                else if (victim.def.IsCorpse)
                {
                    num *= dinfo.Def.corpseDamageFactor;
                }
                damageResult.totalDamageDealt = (float)Mathf.Min(victim.HitPoints, GenMath.RoundRandom(num));
                victim.HitPoints -= Mathf.RoundToInt(damageResult.totalDamageDealt);
                Log.Message($"Dealt {damageResult.totalDamageDealt} to {victim.Label}");
                bool willDie = victim.HitPoints <= 0;
                if (willDie)
                {
                    victim.HitPoints = 0;
                }

                // Post-damage hooks - after damage applied but before death
                foreach (var comp in ExtendedDef.WorkerComps)
                {
                    if (comp.ShouldApply(dinfo, victim))
                    {
                        damageResult = comp.PostApply(dinfo, victim, damageResult);
                    }
                }

                // Handle death and on-killed hooks
                if (willDie)
                {
                    // On-killed hooks - called when damage kills the target
                    foreach (var comp in ExtendedDef.WorkerComps)
                    {
                        if (comp.ShouldApply(dinfo, victim))
                        {
                            comp.OnKilled(dinfo, victim, damageResult);
                        }
                    }

                    victim.Kill(new DamageInfo?(dinfo), null);
                }
            }
            return damageResult;
        }
    }




}