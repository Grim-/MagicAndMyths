using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class EventManager : GameComponent
    {
        private static EventManager instance;
        public static EventManager Instance => instance;

        //private struct QueuedEvent
        //{
        //    public Action Action { get; }
        //    public int Frame { get; }

        //    public QueuedEvent(Action action, int frame)
        //    {
        //        Action = action;
        //        Frame = frame;
        //    }
        //}

        //private Queue<QueuedEvent> eventQueue = new Queue<QueuedEvent>();
        //private readonly object queueLock = new object();

        // Combat events
        public static event Func<Thing, Thing, DamageInfo, DamageWorker.DamageResult, DamageWorker.DamageResult> OnDamageDealt;
        public static event Action<Pawn, DamageInfo> OnPawnDamageTaken;
        public static event Action<Thing, DamageInfo> OnThingDamageTaken;
        public static event Action<Pawn, DamageInfo, Hediff> OnThingKilled;

        // Work events
        public static event Action<Pawn, WorkTypeDef, float> OnWorkCompleted;
        public static event Action<Pawn, SkillDef, float> OnSkillGained;

        // Ability events
        public static event Action<Pawn, Verb> OnVerbUsed;
        public static event Action<Pawn, Ability> OnAbilityCast;
        public static event Action<Pawn, Ability> OnAbilityCompleted;

        // Job events
        public static event Action<Pawn, Job> OnJobStarted;
        public static event Action<Pawn, Job, int> OnJobProgress;
        public static event Action<Pawn, Job, JobCondition> OnJobEnded;
        public static event Action<Pawn, Job, JobCondition> OnJobCleanedUp;


        // Movement and perception events
        public static event Action<Pawn, IntVec3> OnCellEntered;
        public static event Action<Pawn, IntVec3, IntVec3> OnPawnMoved; // From, To
        public static event Func<Pawn, IntVec3, bool> OnPerceptionCheck;
        public EventManager(Game game) : base()
        {
            instance = this;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            //ProcessEvents();
        }


        public static DamageWorker.DamageResult RaiseDamageDealt(Thing target, Thing attacker, DamageInfo dinfo, DamageWorker.DamageResult baseResult)
        {
            return OnDamageDealt?.Invoke(target, attacker, dinfo, baseResult) ?? baseResult;
        }

        public static void RaiseThingDamageTaken(Thing target, DamageInfo info)
        {
            OnThingDamageTaken?.Invoke(target, info);
        }

        public static void RaisePawnDamageTaken(Pawn target, DamageInfo info)
        {
            OnPawnDamageTaken?.Invoke(target, info);
        }

        public static void RaiseOnKilled(Pawn target, DamageInfo info, Hediff culprit = null)
        {
            OnThingKilled?.Invoke(target, info, culprit);
        }

        public static void RaiseWorkCompleted(Pawn pawn, WorkTypeDef workType, float value)
        {
            OnWorkCompleted?.Invoke(pawn, workType, value);
        }

        public static void RaiseSkillGained(Pawn pawn, SkillDef skill, float xp)
        {
            OnSkillGained?.Invoke(pawn, skill, xp);
        }

        public static void RaiseVerbUsed(Pawn pawn, Verb verb)
        {
            OnVerbUsed?.Invoke(pawn, verb);
        }

        public static void RaiseAbilityCast(Pawn pawn, Ability ability)
        {
            OnAbilityCast?.Invoke(pawn, ability);
        }

        public static void RaiseAbilityCompleted(Pawn pawn, Ability ability)
        {
            OnAbilityCompleted?.Invoke(pawn, ability);
        }

        public static void RaiseJobStarted(Pawn pawn, Job job)
        {
            OnJobStarted?.Invoke(pawn, job);
        }

        public static void RaiseJobProgress(Pawn pawn, Job job, int toilIndex)
        {
            OnJobProgress?.Invoke(pawn, job, toilIndex);
        }

        public static void RaiseJobEnded(Pawn pawn, Job job, JobCondition condition)
        {
            OnJobEnded?.Invoke(pawn, job, condition);
        }

        public static void RaiseJobCleanedUp(Pawn pawn, Job job, JobCondition condition)
        {
            OnJobCleanedUp?.Invoke(pawn, job, condition);
        }



        protected static int LastPatherArrivedEventTick = -1;

        public static void PawnArrivedAtPathDestination(Pawn pawn, IntVec3 cell)
        {
            if (Current.Game.tickManager.TicksGame > LastPatherArrivedEventTick + 1)
            {
                OnCellEntered?.Invoke(pawn, cell);
                LastPatherArrivedEventTick = Current.Game.tickManager.TicksGame;
            }
        }

        public static void RaisePawnMoved(Pawn pawn, IntVec3 fromCell, IntVec3 toCell)
        {
            OnPawnMoved?.Invoke(pawn, fromCell, toCell);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref LastPatherArrivedEventTick, "LastPatherArrivedEventTick", -1);
        }
    }
}
