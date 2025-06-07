using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Gene_StanceManager : Gene_BasicResource
    {
        private HashSet<StanceDef> activeStances = new HashSet<StanceDef>();
        private Dictionary<StanceDef, int> stanceCooldowns = new Dictionary<StanceDef, int>();
        private Dictionary<HediffDef, HashSet<StanceDef>> hediffSources = new Dictionary<HediffDef, HashSet<StanceDef>>();
        private Dictionary<AbilityDef, HashSet<StanceDef>> abilitySources = new Dictionary<AbilityDef, HashSet<StanceDef>>();
        private Dictionary<AbilityDef, int> abilityCooldowns = new Dictionary<AbilityDef, int>();

        public IEnumerable<StanceDef> ActiveStances => activeStances;
        public bool HasActiveStances => activeStances.Any();

        public virtual bool IsExclusiveMode
        {
            get
            {
                return true;
            }
        }

        public int MaxConcurrentStances
        {
            get
            {
                return 1;
            }
        }

        public bool AllowStanceReplacement => true;
        public bool AllowStanceDeactivation => true;

        public bool CanActivateStance(StanceDef stance)
        {
            if (stance == null)
                return false;
            if (IsStanceActive(stance))
                return false;
            if (stance.cooldownTicks > 0 && IsStanceOnCooldown(stance))
                return false;

            if (IsExclusiveMode && HasActiveStances)
            {
                return AllowStanceReplacement;
            }

            if (MaxConcurrentStances > 0 && activeStances.Count >= MaxConcurrentStances)
            {
                return AllowStanceReplacement;
            }

            if (stance.activationCost > 0 && !Has(stance.activationCost))
            {
                return false;
            }

            return true;
        }

        public bool CanDeactivateStance(StanceDef stance)
        {
            return IsStanceActive(stance) && (!IsExclusiveMode || AllowStanceDeactivation);
        }

        public bool IsStanceActive(StanceDef stance)
        {
            return activeStances.Contains(stance);
        }

        public bool IsAnyStanceActive()
        {
            return activeStances != null && activeStances.Count > 0;
        }

        public bool IsStanceOnCooldown(StanceDef stance)
        {
            return stanceCooldowns.ContainsKey(stance) &&
                   GenTicks.TicksGame < stanceCooldowns[stance];
        }

        public int GetStanceCooldownRemaining(StanceDef stance)
        {
            if (!IsStanceOnCooldown(stance))
                return 0;
            return Mathf.Max(0, stanceCooldowns[stance] - GenTicks.TicksGame);
        }

        public bool ActivateStance(StanceDef stance, bool consumeResource = true)
        {
            if (!CanActivateStance(stance)) return false;

            if (IsExclusiveMode && HasActiveStances)
            {
                if (AllowStanceReplacement)
                {
                    DeactivateAllStances();
                }
                else
                {
                    return false;
                }
            }

            if (MaxConcurrentStances > 0 && activeStances.Count >= MaxConcurrentStances)
            {
                if (AllowStanceReplacement)
                {
                    var oldestStance = activeStances.First();
                    DeactivateStance(oldestStance);
                }
                else
                {
                    return false;
                }
            }

            if (consumeResource && stance.activationCost > 0)
            {
                Consume(stance.activationCost);
            }

            activeStances.Add(stance);
            ApplyStanceEffects(stance, true);

            if (stance.cooldownTicks > 0)
            {
                stanceCooldowns[stance] = GenTicks.TicksGame + stance.cooldownTicks;
            }

            OnStanceActivated(stance);

            return true;
        }

        public bool DeactivateStance(StanceDef stance)
        {
            if (!CanDeactivateStance(stance)) 
                return false;

            activeStances.Remove(stance);
            ApplyStanceEffects(stance, false);

            OnStanceDeactivated(stance);

            return true;
        }

        public void DeactivateAllStances()
        {
            var stancesToDeactivate = activeStances.ToList();
            foreach (var stance in stancesToDeactivate)
            {
                DeactivateStance(stance);
            }
        }

        private void ApplyStanceEffects(StanceDef stance, bool activate)
        {
            ApplyHediffEffects(stance, activate);

            if (activate)
            {
                AddStanceAbilities(stance);

                if (stance.activationEffecter != null)
                {
                    var effecter = stance.activationEffecter.Spawn();
                    effecter.Trigger(new TargetInfo(pawn), new TargetInfo(pawn));
                    effecter.Cleanup();
                }
            }
            else
            {
                RemoveStanceAbilities(stance);

                if (stance.hediffsToRemoveOnExit != null)
                {
                    foreach (var item in stance.hediffsToRemoveOnExit)
                    {
                        if (this.Pawn.health.hediffSet.HasHediff(item))
                        {
                            this.Pawn.health.RemoveHediff(this.Pawn.health.hediffSet.GetFirstHediffOfDef(item));
                        }
                    }
                }
            }
        }

        private void ApplyHediffEffects(StanceDef stance, bool activate)
        {
            foreach (var hediffDef in stance.hediffsToApply)
            {
                if (activate)
                {
                    if (!hediffSources.ContainsKey(hediffDef))
                    {
                        hediffSources[hediffDef] = new HashSet<StanceDef>();
                    }

                    hediffSources[hediffDef].Add(stance);

                    var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediff == null)
                    {
                        hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        pawn.health.AddHediff(hediff);
                    }
                }
                else
                {
                    if (hediffSources.ContainsKey(hediffDef))
                    {
                        hediffSources[hediffDef].Remove(stance);

                        if (hediffSources[hediffDef].Count == 0)
                        {
                            hediffSources.Remove(hediffDef);
                            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                            if (hediff != null)
                            {
                                pawn.health.RemoveHediff(hediff);
                            }
                        }
                    }
                }
            }
        }

        private void AddStanceAbilities(StanceDef stance)
        {
            foreach (var abilityDef in stance.abilitiesToGain)
            {
                if (!abilitySources.ContainsKey(abilityDef))
                {
                    abilitySources[abilityDef] = new HashSet<StanceDef>();
                }

                abilitySources[abilityDef].Add(stance);

                if (pawn.abilities.GetAbility(abilityDef) == null)
                {
                    pawn.abilities.GainAbility(abilityDef);

                    if (abilityCooldowns.ContainsKey(abilityDef))
                    {
                        var ability = pawn.abilities.GetAbility(abilityDef);
                        if (ability != null)
                        {
                            ability.StartCooldown(abilityCooldowns[abilityDef]);
                        }
                    }
                }
            }
        }

        private void RemoveStanceAbilities(StanceDef stance)
        {
            foreach (var abilityDef in stance.abilitiesToGain)
            {
                if (abilitySources.ContainsKey(abilityDef))
                {
                    abilitySources[abilityDef].Remove(stance);

                    if (abilitySources[abilityDef].Count == 0)
                    {
                        abilitySources.Remove(abilityDef);
                        var ability = pawn.abilities.GetAbility(abilityDef);
                        if (ability != null)
                        {
                            if (ability.CooldownTicksRemaining > 0)
                            {
                                abilityCooldowns[abilityDef] = ability.CooldownTicksRemaining;
                            }
                            else
                            {
                                abilityCooldowns.Remove(abilityDef);
                            }

                            if (ability is IToggleableAbility toggleAbility)
                            {
                                toggleAbility.DeActivate();
                            }

                            pawn.abilities.RemoveAbility(abilityDef);
                        }
                    }
                }
            }
        }

        protected virtual void OnStanceActivated(StanceDef stance)
        {
        }

        protected virtual void OnStanceDeactivated(StanceDef stance)
        {
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(2500))
            {
                foreach (var stance in activeStances.ToList())
                {
                    if (stance.upkeepCost > 0)
                    {
                        if (Has(stance.upkeepCost))
                        {
                            Consume(stance.upkeepCost);
                        }
                        else if (stance.requiresResourceToMaintain)
                        {
                            DeactivateStance(stance);
                        }
                    }
                }

                var expiredCooldowns = abilityCooldowns.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
                foreach (var abilityDef in expiredCooldowns)
                {
                    abilityCooldowns.Remove(abilityDef);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeStances, "activeStances", LookMode.Def);
            Scribe_Collections.Look(ref stanceCooldowns, "stanceCooldowns", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref abilityCooldowns, "abilityCooldowns", LookMode.Def, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                hediffSources.Clear();
                abilitySources.Clear();

                foreach (var stance in activeStances.ToList())
                {
                    ApplyStanceEffects(stance, true);
                }
            }
        }
    }
}