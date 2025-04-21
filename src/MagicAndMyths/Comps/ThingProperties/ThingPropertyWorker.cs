using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThingPropertyDef : Def
    {
        public Type workerClass;
        public float commonality = 1f;
        public bool stackable = false;

        public ThingPropertyWorker CreateWorker(ThingWithComps thing)
        {
            ThingPropertyWorker worker = (ThingPropertyWorker)Activator.CreateInstance(workerClass);
            worker.def = this;
            worker.parent = thing;
            return worker;
        }
    }
    public class ThingComponentsExtension : DefModExtension
    {
        public List<ThingPropertyDef> components = new List<ThingPropertyDef>();
    }

    public abstract class ThingPropertyWorker : IExposable
    {
        public ThingPropertyDef def;
        public ThingWithComps parent;


        public abstract string GetDescription();

        // Basic ThingComp-like methods
        public virtual void PostSpawnSetup(Thing thing, bool respawningAfterLoad)
        {
        }

        public virtual void OnAdded(Thing thing)
        {

        }

        public virtual void OnRemoved(Thing thing)
        {

        }

        public virtual void CompTick(Thing thing)
        {
        }

        public virtual void PostPreApplyDamage(Thing thing, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
        }

        public virtual void PostPostApplyDamage(Thing thing, DamageInfo dinfo, float totalDamageDealt)
        {
        }

        public virtual void PreDestroy(Thing thing)
        {
        }

        public virtual bool IsInvisible(Thing thing)
        {
            return true;
        }

        public virtual float GetStatOffset(StatDef stat)
        {
            return 0;
        }

        public virtual float GetStatFactor(StatDef stat)
        {
            return  1;
        }

        public virtual string GetStatsExplanation(StatDef stat, StringBuilder sb)
        {
          return String.Empty;
        }

        // Event handlers for EventManager events
        // Combat events
        public virtual DamageWorker.DamageResult OnDamageDealt(Thing target, Thing attacker, DamageInfo dinfo, DamageWorker.DamageResult baseResult)
        {
            return baseResult;
        }

        public virtual void OnThingThrown(Thing target, IntVec3 position)
        {

        }

        public virtual void OnThingDamageTaken(Thing target, DamageInfo info)
        {
        }

        public virtual void OnPawnDamageTaken(Pawn target, DamageInfo info)
        {
        }

        public virtual void OnThingKilled(Pawn target, DamageInfo info, Hediff culprit)
        {
        }

        // Work events
        public virtual void OnWorkCompleted(Pawn pawn, WorkTypeDef workType, float value)
        {
        }

        public virtual void OnSkillGained(Pawn pawn, SkillDef skill, float xp)
        {
        }


        public virtual void OnVerbUsed(Pawn pawn, Verb verb)
        {
        }

        public virtual void OnAbilityCast(Pawn pawn, Ability ability)
        {
        }

        public virtual void OnAbilityCompleted(Pawn pawn, Ability ability)
        {
        }


        public virtual void OnJobStarted(Pawn pawn, Job job)
        {
        }

        public virtual void OnJobProgress(Pawn pawn, Job job, int toilIndex)
        {
        }

        public virtual void OnJobEnded(Pawn pawn, Job job, JobCondition condition)
        {
        }

        public virtual void OnJobCleanedUp(Pawn pawn, Job job, JobCondition condition)
        {
        }

        public virtual void OnCellEntered(Pawn pawn, IntVec3 cell)
        {
        }

        public virtual void OnPawnMoved(Pawn pawn, IntVec3 fromCell, IntVec3 toCell)
        {
        }

        public virtual bool OnPerceptionCheck(Pawn pawn, IntVec3 cell)
        {
            return true;
        }

        public virtual void ExposeData()
        {
            
        }
    }
}
