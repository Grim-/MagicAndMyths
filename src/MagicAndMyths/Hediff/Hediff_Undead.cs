using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Hediff_Undead : HediffWithComps, ISquadMember, IRotDrawOverrider
    {
        UndeadHediffDef Def => (UndeadHediffDef)def;

        private Pawn referencedPawn;
        public Pawn Master => referencedPawn;
        public override string Label => base.Label;
        public override string Description => base.Description + $"\nBound to {Master.Label}.";

        public ISquadLeader SquadLeader
        {
            get
            {

                if (Master != null)
                {
                    return Master.GetUndeadMaster();
                }

                return null;
            }
        }
        private SquadMemberState preDefendState = SquadMemberState.CalledToArms;
        private SquadMemberState squadMemberState = SquadMemberState.CalledToArms;
        public SquadMemberState CurrentState => squadMemberState;


        public IntVec3 DefendPoint = IntVec3.Invalid;
        IntVec3 ISquadMember.DefendPoint => DefendPoint;

        public bool HasDefendPoint => DefendPoint != IntVec3.Invalid;

        public Pawn Pawn => this.pawn;


        private bool ShouldOverrideRotDraw = false;
        public bool ShouldOverride => ShouldOverrideRotDraw;

        private RotDrawMode _OverridenRotDrawMode = RotDrawMode.Dessicated;
        public RotDrawMode OverridenRotDrawMode => _OverridenRotDrawMode;

        public void SetSquadLeader(Pawn master)
        { 
            referencedPawn = master;
            ApplyVisual();
        }
        public void SetCurrentMemberState(SquadMemberState newState)
        {
            this.squadMemberState = newState;

            if (this.pawn.CurJob != null)
            {
                if (this.pawn.CurJob != null)
                {
                    this.pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                }
            }
        }

        public void SetDefendPoint(IntVec3 targetPoint)
        {
            this.DefendPoint = targetPoint;
            preDefendState = CurrentState;
            SetCurrentMemberState(SquadMemberState.DefendPoint);
        }

        public void ClearDefendPoint()
        {
            this.DefendPoint = IntVec3.Invalid;
            SetCurrentMemberState(preDefendState);

        }

        public void Notify_SquadMemberAttacked()
        {

        }

        public void Notify_SquadChanged()
        {

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = $"Current State {this.CurrentState}",
                action = () =>
                {
                    List<FloatMenuOption> gridOptions = new List<FloatMenuOption>();
                    gridOptions.Add(new FloatMenuOption("Call To Arms", () =>
                    {
                        this.SetCurrentMemberState(SquadMemberState.CalledToArms);
                    }));

                    gridOptions.Add(new FloatMenuOption("At Ease", () =>
                    {
                        this.SetCurrentMemberState(SquadMemberState.AtEase);
                    }));

                    gridOptions.Add(new FloatMenuOption("Do Nothing", () =>
                    {
                        this.SetCurrentMemberState(SquadMemberState.DoNothing);
                    }));


                    gridOptions.Add(new FloatMenuOption("Fresh", () =>
                    {
                        this._OverridenRotDrawMode = RotDrawMode.Fresh;
                    }));


                    gridOptions.Add(new FloatMenuOption("Dessicated", () =>
                    {
                        this._OverridenRotDrawMode = RotDrawMode.Dessicated;
                    }));

                    gridOptions.Add(new FloatMenuOption("Rotting", () =>
                    {
                        this._OverridenRotDrawMode = RotDrawMode.Rotting;
                    }));

                    Find.WindowStack.Add(new FloatMenu(gridOptions));
                }
            };

            //yield return new Command_Toggle()
            //{
            //    defaultLabel = "Toggle Called to Arms",
            //    Disabled = false,
            //    isActive = () => this.CalledToArms,
            //    icon = TexButton.Add,
            //    toggleAction = () =>
            //    {
            //        this.CalledToArms = !this.IsCalledToArms;
            //    }
            //};

            //yield return new Command_Toggle()
            //{
            //    defaultLabel = "Toggle Colonist Behaviour",
            //    Disabled = false,
            //    isActive = () => this.AllowColonistBehaviour,
            //    icon = TexButton.Add,
            //    toggleAction = () =>
            //    {
            //        this.AllowColonistBehaviour = !this.AllowColonistBehaviour;
            //    }
            //};
        }
        private void ApplyVisual()
        {
            if (this.pawn.story != null)
            {
               // this.pawn.story.skinColorOverride = Color.black;
                if (this.pawn.RaceProps.Humanlike)
                {
                    this.pawn.story.HairColor = new Color(0.85f, 0.85f, 0.85f);
                }       
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.pawn != null && !this.pawn.Dead && !this.pawn.Destroyed && this.Def != null && this.pawn.IsHashIntervalTick(Def.regenTicks))
            {
                this.pawn.QuickHeal(Def.baseHealAmount);
                HandleNeeds();

                if (this.Master == null)
                {
                    Log.Message("Undead master is null removing");

                    this.Severity = 0;
                }
            }
        }


        public void TurnFeral()
        {

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (this.Master != null)
            {
                this.Master.GetUndeadMaster()?.RemoveFromSquad(this.pawn, true);
            }
            base.Notify_PawnDied(dinfo, culprit);
        }

        private void HandleNeeds()
        {
            foreach (var item in this.pawn.needs.AllNeeds)
            {
                item.CurLevel = item.MaxLevel;
            }

            if (this.pawn.IsSlave)
            {
                Need_Suppression suppression = this.pawn.needs.TryGetNeed<Need_Suppression>();
                if (suppression != null)
                {
                    suppression.CurLevel = suppression.MaxLevel;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
            Scribe_Values.Look(ref squadMemberState, "squadMemberState");
            Scribe_Values.Look(ref preDefendState, "preDefendState");
            Scribe_Values.Look(ref DefendPoint, "defendPoint");
        }
    }


    public interface IRotDrawOverrider
    {
        bool ShouldOverride { get; }
        RotDrawMode OverridenRotDrawMode { get; }
    }
}
