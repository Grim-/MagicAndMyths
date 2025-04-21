using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{

    public class CompProperties_ThingProperties : CompProperties
    {
        public CompProperties_ThingProperties()
        {
            compClass = typeof(Comp_ThingProperties);
        }
    }

    public class Comp_ThingProperties : ThingComp
    {
        private Dictionary<ThingPropertyDef, ThingPropertyWorker> activeComponents = new Dictionary<ThingPropertyDef, ThingPropertyWorker>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitializeComponents();

            if (activeComponents.Count > 0)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.PostSpawnSetup(parent, respawningAfterLoad);
                }
            }

            RegisterEventHandlers();
        }

        private void InitializeComponents()
        {
            if (activeComponents.Count == 0 && parent.def.GetModExtension<ThingComponentsExtension>() is ThingComponentsExtension extension)
            {
                foreach (ThingPropertyDef componentDef in extension.components)
                {
                    AddProperty(componentDef);
                }
            }
        }

        public virtual ThingPropertyWorker AddProperty(ThingPropertyDef thingPropertyDef)
        {
            if (activeComponents.ContainsKey(thingPropertyDef))
            {
                Log.Message("Has Property Already");
                return null;
            }

            ThingPropertyWorker worker = thingPropertyDef.CreateWorker(this.parent);
            activeComponents.Add(thingPropertyDef, worker);
            worker.OnAdded(parent);
            Log.Message($"Added {thingPropertyDef.LabelCap} to {this.parent.Label}");
            return worker;
        }

        public virtual void RemoveProperty(ThingPropertyDef thingPropertyDef)
        {
            if (!activeComponents.ContainsKey(thingPropertyDef))
            {
                return;
            }

            ThingPropertyWorker thingPropertyWorker = GetProperty(thingPropertyDef);

            if (thingPropertyWorker != null)
            {
                thingPropertyWorker.OnRemoved(parent);
            }


            activeComponents.Remove(thingPropertyDef);
        }

        public ThingPropertyWorker GetProperty(ThingPropertyDef thingPropertyDef)
        {
            if (!activeComponents.ContainsKey(thingPropertyDef))
            {
                return null;
            }

            return activeComponents[thingPropertyDef];
        }


        public override float GetStatOffset(StatDef stat)
        {
            float offset = 0;

            if (activeComponents != null)
            {
                foreach (var item in activeComponents.Values)
                {
                    float value = item.GetStatOffset(stat);
                    if (value != 0)
                    {
                        offset += value;
                    }
                }
            }

            return base.GetStatOffset(stat) + offset;
        }

        public override float GetStatFactor(StatDef stat)
        {
            float factor = 1f;

            if (activeComponents != null)
            {
                foreach (var item in activeComponents.Values)
                {
                    float value = item.GetStatFactor(stat);
                    if (value != 1f)
                    {
                        factor *= value;
                    }
                }
            }

            return base.GetStatFactor(stat) * factor;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
        {
            base.GetStatsExplanation(stat, sb);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (activeComponents.Count > 0)
            {
                sb.AppendLine($"Properties - ");
                foreach (var item in activeComponents)
                {
                    sb.AppendLine($"    {item.Key.LabelCap}");
                }
            }
            return base.CompInspectStringExtra() + sb.ToString();
        }

        private void RegisterEventHandlers()
        {
            EventManager.OnDamageDealt += HandleDamageDealt;
            EventManager.OnThingDamageTaken += HandleThingDamageTaken;

            if (parent is Pawn)
            {
                EventManager.OnPawnDamageTaken += HandlePawnDamageTaken;
                EventManager.OnThingKilled += HandleThingKilled;
                EventManager.OnWorkCompleted += HandleWorkCompleted;
                EventManager.OnSkillGained += HandleSkillGained;
                EventManager.OnVerbUsed += HandleVerbUsed;
                EventManager.OnAbilityCast += HandleAbilityCast;
                EventManager.OnAbilityCompleted += HandleAbilityCompleted;
                EventManager.OnJobStarted += HandleJobStarted;
                EventManager.OnJobProgress += HandleJobProgress;
                EventManager.OnJobEnded += HandleJobEnded;
                EventManager.OnJobCleanedUp += HandleJobCleanedUp;
                EventManager.OnCellEntered += HandleCellEntered;
                EventManager.OnPawnMoved += HandlePawnMoved;
                EventManager.OnPerceptionCheck += HandlePerceptionCheck;
            }
        }

        private void UnregisterEventHandlers()
        {

            EventManager.OnDamageDealt -= HandleDamageDealt;
            EventManager.OnThingDamageTaken -= HandleThingDamageTaken;
            if (parent is Pawn)
            {
                EventManager.OnPawnDamageTaken -= HandlePawnDamageTaken;
                EventManager.OnThingKilled -= HandleThingKilled;
                EventManager.OnWorkCompleted -= HandleWorkCompleted;
                EventManager.OnSkillGained -= HandleSkillGained;
                EventManager.OnVerbUsed -= HandleVerbUsed;
                EventManager.OnAbilityCast -= HandleAbilityCast;
                EventManager.OnAbilityCompleted -= HandleAbilityCompleted;
                EventManager.OnJobStarted -= HandleJobStarted;
                EventManager.OnJobProgress -= HandleJobProgress;
                EventManager.OnJobEnded -= HandleJobEnded;
                EventManager.OnJobCleanedUp -= HandleJobCleanedUp;
                EventManager.OnCellEntered -= HandleCellEntered;
                EventManager.OnPawnMoved -= HandlePawnMoved;
                EventManager.OnPerceptionCheck -= HandlePerceptionCheck;
            }
        }

        // Event Handlers
        private DamageWorker.DamageResult HandleDamageDealt(Thing target, Thing attacker, DamageInfo dinfo, DamageWorker.DamageResult baseResult)
        {
            if (target == parent)
            {
                DamageWorker.DamageResult result = baseResult;
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    result = component.OnDamageDealt(target, attacker, dinfo, result);
                }
                return result;
            }
            return baseResult;
        }

        private void HandleThingDamageTaken(Thing target, DamageInfo info)
        {
            if (target == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnThingDamageTaken(target, info);
                }
            }
        }

        private void HandlePawnDamageTaken(Pawn target, DamageInfo info)
        {
            if (target == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnPawnDamageTaken(target, info);
                }
            }
        }

        private void HandleThingKilled(Pawn target, DamageInfo info, Hediff culprit)
        {
            if (target == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnThingKilled(target, info, culprit);
                }
            }
        }

        private void HandleWorkCompleted(Pawn pawn, WorkTypeDef workType, float value)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnWorkCompleted(pawn, workType, value);
                }
            }
        }

        private void HandleSkillGained(Pawn pawn, SkillDef skill, float xp)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnSkillGained(pawn, skill, xp);
                }
            }
        }

        private void HandleVerbUsed(Pawn pawn, Verb verb)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnVerbUsed(pawn, verb);
                }
            }
        }

        private void HandleAbilityCast(Pawn pawn, Ability ability)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnAbilityCast(pawn, ability);
                }
            }
        }

        private void HandleAbilityCompleted(Pawn pawn, Ability ability)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnAbilityCompleted(pawn, ability);
                }
            }
        }

        private void HandleJobStarted(Pawn pawn, Job job)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnJobStarted(pawn, job);
                }
            }
        }

        private void HandleJobProgress(Pawn pawn, Job job, int toilIndex)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnJobProgress(pawn, job, toilIndex);
                }
            }
        }

        private void HandleJobEnded(Pawn pawn, Job job, JobCondition condition)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnJobEnded(pawn, job, condition);
                }
            }
        }

        private void HandleJobCleanedUp(Pawn pawn, Job job, JobCondition condition)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnJobCleanedUp(pawn, job, condition);
                }
            }
        }

        private void HandleCellEntered(Pawn pawn, IntVec3 cell)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnCellEntered(pawn, cell);
                }
            }
        }

        private void HandlePawnMoved(Pawn pawn, IntVec3 fromCell, IntVec3 toCell)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    component.OnPawnMoved(pawn, fromCell, toCell);
                }
            }
        }

        private bool HandlePerceptionCheck(Pawn pawn, IntVec3 cell)
        {
            if (pawn == parent)
            {
                foreach (ThingPropertyWorker component in activeComponents.Values)
                {
                    if (!component.OnPerceptionCheck(pawn, cell))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void CompTick()
        {
            base.CompTick();


            foreach (var item in activeComponents.Values)
            {
                item.CompTick(parent);
            }
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            foreach (var item in activeComponents.Values)
            {
                item.PostPreApplyDamage(parent, ref dinfo, out bool compAbsorbed);
                if (compAbsorbed)
                {
                    absorbed = true;
                    return;
                }
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            foreach (var item in activeComponents.Values)
            {
                item.PostPostApplyDamage(parent, dinfo, totalDamageDealt);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            UnregisterEventHandlers();
            foreach (var item in activeComponents.Values)
            {
                item.PreDestroy(parent);
            }
            base.PostDestroy(mode, previousMap);
        }

        public bool ShouldBeVisible()
        {
            foreach (var item in activeComponents.Values)
            {
                if (item.IsInvisible(parent))
                {
                    return false;
                }
            }

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref activeComponents, "activeComponents", LookMode.Deep);
        }
    }
}
